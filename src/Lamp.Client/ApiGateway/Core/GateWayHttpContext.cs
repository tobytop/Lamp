using Microsoft.AspNetCore.Http;

namespace Lamp.Client.ApiGateway.Core
{
    public class GateWayHttpContext
    {
        private static IHttpContextAccessor _accessor;

        public static HttpContext Current => _accessor.HttpContext;

        internal static void Configure(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }
    }
}
