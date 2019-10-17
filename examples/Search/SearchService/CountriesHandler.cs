using LiteDB;
using Newtonsoft.Json;
using ResgateIO.Service;
using System.Collections.Generic;
using System.IO;

namespace SearchService
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
    class CountriesHandler: BaseHandler
    {
        public void Access(IAccessRequest req)
        {
            req.AccessGranted();
        }

        public void Get(ICollectionRequest req)
        {
            req.Collection(Countries.List);
        }
    }
}
