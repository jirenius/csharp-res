using LiteDB;
using Newtonsoft.Json;
using ResgateIO.Service;
using System.Collections.Generic;
using System.IO;

namespace SearchService
{
    [ResourcePattern("customer.$id")]
    [ResourceGroup("customers")]
    class CustomerHandler: BaseHandler
    {
        private readonly LiteCollection<Customer> customers;

        public CustomerHandler(LiteDatabase db) : base()
        {
            customers = db.GetCollection<Customer>("customers");

            if (!db.CollectionExists("customers"))
            {
                populateDBWithDefaultData();
            }

            customers.EnsureIndex("Name", "LOWER($.Name)", false);
        }

        public void Access(IAccessRequest req)
        {
            req.AccessGranted();
        }

        public void Get(IModelRequest req)
        {
            var customer = customers.FindById(new ObjectId(req.PathParam("id")));
            if (customer == null)
                req.NotFound();
            else
                req.Model(customer);
        }

        [CallMethod("set")]
        public void Set(ICallRequest req)
        {
            Customer customerParams = req.ParseParams<Customer>();
           
            // Preprocess call params
            customerParams.TrimAll();

            // Validate call params
            if (customerParams.HasError(out string errorMsg))
            {
                req.InvalidParams(errorMsg);
                return;
            }

            // Populate dictionary with updated fields
            var changed = new Dictionary<string, object>(3);
            if (customerParams.Name != null)
                changed["name"] = customerParams.Name;
            if (customerParams.Email != null)
                changed["email"] = customerParams.Email;
            if (customerParams.Country != null)
                changed["country"] = customerParams.Country;

            // Send a change event with updated fields
            req.ChangeEvent(changed);

            // Send success response
            req.Ok();
        }

        [CallMethod("delete")]
        public void Delete(ICallRequest req)
        {
            // Send a change event with updated fields
            req.DeleteEvent();

            // Send success response
            req.Ok();
        }

        // Applies the changes to the database.
        public void ApplyChange(IResourceContext resource, ChangeEventArgs ev)
        {
            var customer = resource.RequireValue<Customer>();
            var revert = new Dictionary<string, object>(ev.ChangedProperties.Count);

            // Apply Name
            if (ev.ChangedProperties.TryGetValue("name", out object nameObject))
            {
                string name = (string)nameObject;
                if (name != customer.Name)
                {
                    revert["name"] = customer.Name;
                    customer.Name = name;
                }
            }

            // Apply Email
            if (ev.ChangedProperties.TryGetValue("email", out object emailObject))
            {
                string email = (string)emailObject;
                if (email != customer.Email)
                {
                    revert["email"] = customer.Email;
                    customer.Email = email;
                }
            }

            // Apply Country
            if (ev.ChangedProperties.TryGetValue("country", out object countryObject))
            {
                string country = (string)countryObject;
                if (country != customer.Country)
                {
                    revert["country"] = customer.Country;
                    customer.Country = country;
                }
            }

            // Update the database if there are any changes to apply
            if (revert.Count > 0)
            {
                customers.Update(customer);
            }

            ev.SetRevert(revert);
        }

        // Applies the create to the database.
        public void ApplyCreate(IResourceContext resource, CreateEventArgs ev)
        {
            customers.Insert((Customer)ev.Data);
        }

        // Applies the delete to the database if it existed,
        // or else throws a ResError.NotFound ResException.
        public void ApplyDelete(IResourceContext resource, DeleteEventArgs ev)
        {
            var id = new ObjectId(resource.PathParam("id"));
            var customer = customers.FindById(id);
            if (customer == null)
            {
                throw new ResException(ResError.NotFound);
            }
            customers.Delete(Query.EQ("_id", id));
            ev.SetRevert(customer);
        }

        private void populateDBWithDefaultData()
        {
            List<Customer> list = JsonConvert.DeserializeObject<List<Customer>>(File.ReadAllText(@"mock_customers.json", System.Text.Encoding.UTF8));
            customers.InsertBulk(list);
        }
    }
}
