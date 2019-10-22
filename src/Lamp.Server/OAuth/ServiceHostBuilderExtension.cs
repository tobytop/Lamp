using Autofac;
using Lamp.Core;
using Lamp.Core.Common.Logger;
using Lamp.Core.OAuth;
using Lamp.Core.Protocol.Server;
using Lamp.Core.Serializer;
using Lamp.Core.Server;
using Lamp.Core.Server.Discovery;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Lamp.Server.OAuth
{
    public static partial class ServiceHostBuilderExtension
    {
        public static IServiceHostServerBuilder UseJwtForOAuth(this IServiceHostServerBuilder serviceHostBuilder, JwtAuthorizationOptions options)
        {
            serviceHostBuilder.AddRunner(container =>
            {
                ILogger logger = container.Resolve<ILogger>();
                logger.Info($"[config]use jwt for OAuth");

                while (!container.IsRegistered<IServer>() || !container.IsRegistered<IServiceDiscovery>())
                {
                    Thread.Sleep(200);
                }
                IServer server = container.Resolve<IServer>();
                ISerializer serializer = container.Resolve<ISerializer>();
                server.UseMiddleware<JwtAuthorizationMiddleware>(options, serializer);

                if (string.IsNullOrEmpty(options.TokenEndpointPath))
                {
                    return;
                }

                if (options.ServerIp == "localhost" || options.ServerIp == "127.0.0.1")
                {
                    IServiceDiscovery discovery = container.Resolve<IServiceDiscovery>();
                    ServerAddress addr = new ServerAddress(options.ServerIp, options.ServerPort);

                    List<ServiceRoute> tokenRoute = new List<ServiceRoute> {
                        new ServiceRoute
                        {
                            Address = new List<ServerAddress>{
                                addr
                            },
                            ServiceDescriptor = new ServiceDesc
                            {
                                Id = options.TokenEndpointPath.TrimStart('/'),
                                RoutePath = options.TokenEndpointPath,
                                HttpMethod="Post",
                                Parameters = serializer.Serialize<string>(new List<ServiceParameterDesc>{
                                     new ServiceParameterDesc
                                     {
                                          Comment = "username",
                                          Format = "System.String",
                                          Name = "username",
                                          Type = "object"
                                     },
                                     new ServiceParameterDesc
                                     {
                                          Comment = "password",
                                          Format = "System.String",
                                          Name = "password",
                                          Type = "object"
                                     },
                                 }),
                                ReturnDesc = serializer.Serialize<string>( new ServiceReturnDesc{
                                     Comment = "Token",
                                     ReturnType = "object",
                                     ReturnFormat = "{\"access_token\":\"System.String | token\", \"expired_in\":\"System.Int32 | expired timestamp which is the number of seconds between 1970-01-01 and expired datetime\"}"
                                })
                            }
                        }
                    };
                    discovery.ClearServiceAsync(tokenRoute.First().ServiceDescriptor.Id).Wait();
                    //discovery.SetRoutesAsync(tokenRoute);
                    discovery.AddRouteAsync(tokenRoute).Wait();
                }
            });
            return serviceHostBuilder;
        }
    }
}
