using System;

namespace Lamp.Core.Client.Token.Implement
{
    public class ServiceTokenGetter : IServiceTokenGetter
    {
        public Func<string> GetToken { get; set; }
    }
}
