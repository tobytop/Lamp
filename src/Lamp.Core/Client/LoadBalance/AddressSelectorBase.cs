using Lamp.Core.Protocol.Server;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lamp.Core.Client.LoadBalance
{
    public abstract class AddressSelectorBase : IAddressSelector
    {
        Task<ServerAddress> IAddressSelector.GetAddressAsync(List<ServerAddress> serverDesc)
        {
            if (serverDesc == null)
            {
                throw new ArgumentNullException(nameof(serverDesc));
            }

            return GetAddressAsyn(serverDesc);
        }

        public abstract Task<ServerAddress> GetAddressAsyn(List<ServerAddress> serverDesc);
    }
}
