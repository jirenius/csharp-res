using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ResgateIO.Service
{
    /// <summary>
    /// Static action values.
    /// </summary>
    public static class ResAction
    {
        /// <summary>
        /// Delete action is used with model change events when a property has been deleted from a model.
        /// </summary>
        /// <see>https://resgate.io/docs/specification/res-service-protocol/#delete-action</see>
        public static readonly JRaw Delete = new JRaw("{\"action\":\"delete\"}");
    }
}
