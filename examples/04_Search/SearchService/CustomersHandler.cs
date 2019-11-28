using LiteDB;
using ResgateIO.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace SearchService
{
    [ResourcePattern("customers")]
    [ResourceGroup("customers")]
    public class CustomersHandler : BaseHandler
    {
        private readonly LiteCollection<Customer> customers;

        public CustomersHandler(LiteDatabase db) : base()
        {
            customers = db.GetCollection<Customer>("customers");
        }

        public void Access(IAccessRequest req)
        {
            req.AccessGranted();
        }

        public void Get(ICollectionRequest req)
        {
            var q = HttpUtility.ParseQueryString(req.Query);
            var name = q.Get("name")?.ToLower();
            var country = q.Get("country");
            int.TryParse(q.Get("from"), out int from);
            int.TryParse(q.Get("limit"), out int limit);
            if (from < 0) from = 0;
            if (limit <= 0) limit = 10;

            // Put a max on the query result size
            if (limit > 50)
            {
                req.InvalidQuery("Limit must be 50 or less.");
                return;
            }

            IEnumerable<Customer> result = null;            
            if (String.IsNullOrEmpty(name))
                if (String.IsNullOrEmpty(country))
                    result = customers.Find(Query.All("Name"), from, limit);
                else
                    result = customers.Find(Query.And(Query.All("Name"), Query.Where("Country", v => v.AsString == country)), from, limit);
            else
                if (String.IsNullOrEmpty(country))
                    result = customers.Find(Query.StartsWith("Name", name), from, limit);
                else
                    result = customers.Find(Query.And(Query.StartsWith("Name", name), Query.Where("Country", v => v.AsString == country)), from, limit);

            // Create a normalized query string with the properties in a set order.
            var normalizedQuery = String.Format("name={0}&country={1}&from={2}&limit={3}", name ?? "", country ?? "", from, limit);
            req.Collection(result.Select(c => new Ref("search.customer." + c.Id)), normalizedQuery);
        }

        [CallMethod("newCustomer")]
        public void NewCustomer(ICallRequest req)
        {
            Customer customer = req.ParseParams<Customer>();

            // Preprocess call params
            customer.TrimAll();

            // Set default values for missing values
            customer.Name = customer.Name ?? "";
            customer.Email = customer.Email ?? "";
            customer.Country = customer.Country ?? "";

            // Validate call params
            if (customer.HasError(out string errorMsg))
            {
                req.InvalidParams(errorMsg);
                return;
            }

            // Create a new Customer ID
            customer.CustomerId = ObjectId.NewObjectId();
            var rid = "search.customer." + customer.Id;

            req.Service.With(rid, r =>
            {
                // Send a create event. The ApplyCreate will store it in LiteDB.
                r.CreateEvent(customer);
            });

            // Send success response with new resource ID
            req.Resource(rid);
        }

        // Listen to customer's change, create and delete events
        // to see if these events may have affected any queries.
        [EventListener("customer.$id")]
        private void onCustomersEvent(object sender, EventArgs ev)
        {
            var r = (IResourceContext)sender;
            Customer customer;
            switch (ev)
            {
                case ChangeEventArgs change:
                    // Only changes to name or country can affect a query. Quick exit in other cases.
                    if (!change.ChangedProperties.ContainsKey("name") && !change.ChangedProperties.ContainsKey("country"))
                    {
                        return;
                    }
                    customer = r.RequireValue<Customer>();
                    sendQueryEvent(r, query => possiblyAffectedByChange(customer, query, change));
                    break;

                case CreateEventArgs create:
                    customer = r.RequireValue<Customer>();
                    sendQueryEvent(r, query => possiblyAffectedByCreateDelete(customer, query));
                    break;

                case DeleteEventArgs delete:
                    sendQueryEvent(r, query => possiblyAffectedByCreateDelete((Customer)delete.Data, query));
                    break;
            }
        }

        private void sendQueryEvent(IResourceContext r, Predicate<string> possiblyAffected) {
            r.Service.Resource("search.customers").QueryEvent(qreq =>
            {
                // A null value signals the end of the query event.
                // As we have nothing related to dispose, we do a quick exit.
                if (qreq == null) return;
                // By making a preliminary check if a query is possibly affected or not,
                // we can avoid doing unneccesary database searches.
                // Eg. a deleted person from Sweden will not affect a country=France query.
                if (possiblyAffected(qreq.Query))
                {
                    // Respond with what that query collection looks like now.
                    // RequireValue will use the Get method above to get the collection.
                    qreq.Collection(qreq.RequireValue<IEnumerable<Ref>>());
                }
            });
        }

        // Determines if a change event might have affected the result of a query.
        private bool possiblyAffectedByChange(Customer customer, string query, ChangeEventArgs ev)
        {
            var q = HttpUtility.ParseQueryString(query);
            var nameQuery = q.Get("name")?.ToLower();
            var countryQuery = q.Get("country");

            var oldName = getStringOrDefault(ev.OldProperties, "name", customer.Name);
            var oldCountry = getStringOrDefault(ev.OldProperties, "country", customer.Country);
            var newName = getStringOrDefault(ev.ChangedProperties, "name", customer.Name);
            var newCountry = getStringOrDefault(ev.ChangedProperties, "country", customer.Country);

            var wasMatch = matchesQuery(oldName, oldCountry, nameQuery, countryQuery);
            var isMatch = matchesQuery(newName, newCountry, nameQuery, countryQuery);

            // Might be affected if:
            // * Query match has changed due to the change event
            // * Or the sort order might have changed due to a name change
            return (wasMatch != isMatch)
                || (isMatch && ev.ChangedProperties.ContainsKey("name"));
        }

        // Determines if a create or delete event might have affected the result of a query.
        private bool possiblyAffectedByCreateDelete(Customer customer, string query)
        {
            var q = HttpUtility.ParseQueryString(query);
            var nameQuery = q.Get("name")?.ToLower();
            var countryQuery = q.Get("country");

            return matchesQuery(customer.Name, customer.Country, nameQuery, countryQuery);
        }

        // Gets a string value from a change dictionary, or default string if it doesn't exist.
        private string getStringOrDefault(Dictionary<string, object> dict, string key, string defaultString)
        {
            if (dict.TryGetValue(key, out object value))
                return (string)value;
            else
                return defaultString;
        }

        // Checks if a name and a country matches a query.
        private bool matchesQuery(string name, string country, string nameQuery, string countryQuery)
        {
            return (
                String.IsNullOrEmpty(nameQuery) ||
                name.ToLower().StartsWith(nameQuery)
            ) && (
                String.IsNullOrEmpty(countryQuery) ||
                country == countryQuery
            );
        }
    }
}
