using Lamp.Core.Client.Discovery;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;

namespace Lamp.Swagger
{
    public static class StartupExtension
    {
        public static IApplicationBuilder UseRpcSwagger(this IApplicationBuilder app, params string[] version)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                if (version.Length == 0)
                {
                    c.SwaggerEndpoint($"/swagger/all/swagger.json", $"Rpc API");
                }
                else
                {
                    foreach (string v in version)
                    {
                        c.SwaggerEndpoint($"/swagger/{v}/swagger.json", $"Rpc API {v}");
                    }
                }
            });

            return app;
        }
        public static IServiceCollection UseRpcSwagger(this IServiceCollection services, RpcSwaggerOptions options, IClientServiceDiscovery clientServiceDiscovery)
        {
            services.AddSwaggerGen(c =>
            {
                c.CustomSchemaIds(type => type.FullName);
                if (options.Version.Length == 0)
                {
                    c.SwaggerDoc("all", new Info { Title = options.Title, Version = "all" });
                }
                else
                {
                    foreach (string v in options.Version)
                    {
                        c.SwaggerDoc(v, new Info { Title = options.Title, Version = v });
                    }
                }

                c.DocumentFilter<RpcSwaggerDocumentFilter>(clientServiceDiscovery);
            });

            return services;
        }
    }
}
