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

        public void New(INewRequest req)
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
            req.New(new Ref(rid));
        }

        // Listen to customers create and delete events
        [EventListener("customer.$id")]
        private void onCustomersEvent(object sender, EventArgs ev)
        {
            var r = (IResourceContext)sender;
            switch (ev)
            {
                case ChangeEventArgs change:
                    // Only reset the query if the change in the customer may affect the query.
                    if (change.ChangedProperties.ContainsKey("name") || change.ChangedProperties.ContainsKey("country"))
                    {
                        r.Service.Reset(new[] { "search.customers" }, null);
                    }
                    break;

                case CreateEventArgs create:
                case DeleteEventArgs delete:
                    // Reset if a customer was created or deleted
                    r.Service.Reset(new[] { "search.customers" }, null);
                    break;
            }
        }
    }
}
