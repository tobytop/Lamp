using Microsoft.AspNetCore.Mvc;
using System.Collections.Specialized;
using System.Web;

namespace Lamp.Client.ApiGateway.Core
{
    [ModelBinder(BinderType = typeof(GateWayQueryStringModelBinder))]
    public class GateWayQueryString
    {
        public string Query { get; set; }

        public NameValueCollection Collection { get; set; }
        public GateWayQueryString(string query)
        {
            Query = query;
            Collection = HttpUtility.ParseQueryString(query);
        }
    }
}
