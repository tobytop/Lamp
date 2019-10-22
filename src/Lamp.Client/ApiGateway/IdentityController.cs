using Lamp.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Lamp.Client.ApiGateway
{
    [Authorize]
    public class IdentityController : ControllerBase
    {
        [Route(Config.ClientAuthorizationUrl)]
        [HttpGet]
        public IActionResult Get()
        {
            return new JsonResult(from c in User.Claims select new { c.Type, c.Value });
        }
    }
}
