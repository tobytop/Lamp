using Autofac;
using Autofac.Extensions.DependencyInjection;
using Lamp.Client;
using Lamp.Client.ApiGateway;
using Lamp.Client.ApiGateway.Core;
using Lamp.Client.Cache;
using Lamp.Client.Consul;
using Lamp.Client.LoadBalance;
using Lamp.Core;
using Lamp.Core.Client.LoadBalance;
using Lamp.Core.Client.Token;
using Lamp.Core.Protocol.Server;
using Lamp.Swagger;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace Lamp.Test.Web
{
    public class Startup
    {
        private readonly ContainerBuilder _containerBuilder;
        private IServiceHost _host;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            _containerBuilder = new ContainerBuilder();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            //services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            _host = new ServiceHostClientBuilder(_containerBuilder)
                //.UseInServerForDiscovery(new ServerAddress("127.0.0.1", 8007, 3), new ServerAddress("127.0.0.1", 8008))
                .UseConsulForDiscovery(new RegisterServer("127.0.0.1", 8500), new ServerAddress("127.0.0.1", 8008))
                .UseRpcForTransfer()
                .UseToken(() =>
                {
                    var headers = GateWayHttpContext.Current.Request.Headers["Authorization"];
                    return headers.Any() ? headers[0] : null;
                })
                .UsePollingAddressSelector(BalanceType.RoundRobin)
                //.UseAutofacForMvc(services)
                .UseSwaggerAndGateWay(new RpcSwaggerOptions("我的API"), services)
                .UseRemoteCache(20)
                //.SetDiscoveryAutoUpdateJobInterval(60)
                .Build();

            //services.UseRpcSwagger(new RpcSwaggerOptions("我的API"));

            return new AutofacServiceProvider(_host.Container);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();

            app.UseRpcSwagger();
            app.UseGateWay(_host);

            _host.Run();
        }
    }
}
