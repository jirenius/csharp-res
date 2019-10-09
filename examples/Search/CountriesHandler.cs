using LiteDB;
using Newtonsoft.Json;
using ResgateIO.Service;
using System.Collections.Generic;
using System.IO;

namespace Search
{
    public static class Countries
    {
        public static readonly List<string> List = new List<string>
        {
            "France",
            "Germany",
            "Sweden",
            "United Kingdom"
        };
    }

    [ResourcePattern("countries")]
    class CountriesHandler: CollectionHandler
    {
        public override void Access(IAccessRequest req)
        {
            req.AccessGranted();
        }

        public override void Get(ICollectionRequest req)
        {
            req.Collection(Countries.List);
        }
    }
}
