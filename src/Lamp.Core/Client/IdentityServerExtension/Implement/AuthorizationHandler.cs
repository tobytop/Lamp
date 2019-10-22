using System;
using System.Collections.Generic;

namespace Lamp.Core.Client.IdentityServerExtension.Implement
{
    public class AuthorizationHandler : IAuthorizationHandler
    {
        public Func<string, string, IDictionary<string, object>> GetAuthorizationContext { get; set; }
    }
}
