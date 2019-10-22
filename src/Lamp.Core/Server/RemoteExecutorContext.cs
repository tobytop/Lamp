using Lamp.Core.Common.Logger;
using Lamp.Core.Protocol.Communication;
using Lamp.Core.Protocol.Server;
using Lamp.Core.Serializer;
using Lamp.Core.Server.Discovery;
using Lamp.Core.Server.ServiceContainer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lamp.Core.Server
{
    public class RemoteExecutorContext
    {
        public RemoteExecutorContext(TransportMsg transportMessage, IServiceEntryContainer serviceEntryContainer,
            IResponse response, ISerializer serializer, ILogger logger, IServiceDiscovery serviceDiscovery)
        {
            Response = response;
            TransportMessage = transportMessage;
            try
            {
                RemoteInvokeMessage = transportMessage.GetContent<RemoteCallData>(serializer);
            }
            catch (Exception ex)
            {
                logger.Error("failed to convert transportmsg.content to  RemoteCallerData.", ex);
                return;
            }

            ServiceEntry = serviceEntryContainer.GetServiceEntry()
                .FirstOrDefault(x => x.Descriptor.Id == RemoteInvokeMessage.ServiceId);
            ServiceDesc serviceDesc;
            if (ServiceEntry == null)
            {
                logger.Warn($"not found service: {RemoteInvokeMessage.ServiceId}");
                List<ServiceRoute> router = serviceDiscovery.GetRoutesAsync().Result;
                serviceDesc = router.FirstOrDefault(o => o.ServiceDescriptor.Id == RemoteInvokeMessage.ServiceId).ServiceDescriptor;
            }
            else
            {
                serviceDesc = ServiceEntry.Descriptor;
            }

            RemoteInvokeMessage.Parameters = CheckParameters(RemoteInvokeMessage.Parameters, serviceDesc, serializer);
        }

        private IDictionary<string, object> CheckParameters(IDictionary<string, object> parameters, ServiceDesc serviceDesc, ISerializer serializer)
        {
            List<ServiceParameterDesc> serviceParameters = serializer.Deserialize<string, List<ServiceParameterDesc>>(serviceDesc.Parameters);
            if (serviceParameters.Count == 1 && !parameters.ContainsKey(serviceParameters[0].Name))
            {
                return new Dictionary<string, object>
                {
                    {serviceParameters[0].Name,parameters }
                };
            }
            else
            {
                return parameters;
            }
        }

        public ServiceEntry ServiceEntry { get; }
        public IResponse Response { get; }

        public TransportMsg TransportMessage { get; }

        public RemoteCallData RemoteInvokeMessage { get; }
    }
}
