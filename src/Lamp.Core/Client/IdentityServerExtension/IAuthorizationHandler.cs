using System;
using System.Collections.Generic;

namespace Lamp.Core.Client.IdentityServerExtension
{
    public interface IAuthorizationHandler
    {
        Func<string, string, IDictionary<string, object>> GetAuthorizationContext { get; set; }
    }
}
