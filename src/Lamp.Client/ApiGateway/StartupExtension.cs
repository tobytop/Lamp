using Lamp.Client.ApiGateway.Core;
using Lamp.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Serialization;

namespace Lamp.Client.ApiGateway
{
    public static class StartupExtension
    {
        public static IApplicationBuilder UseGateWay(this IApplicationBuilder app, IServiceHost host)
        {
            app.UseMiddleware<HttpStatusCodeExceptionMiddleware>();
            app.UseCors(builder =>
            {
                builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
            });

            IHttpContextAccessor httpContextAccessor = app.ApplicationServices.GetRequiredService<IHttpContextAccessor>();

            GateWayHttpContext.Configure(httpContextAccessor);

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action}");
                routes.MapRoute(
                    name: "defaultApi",
                    template: "api/{controller}/{action}");
                routes.MapRoute(
                   "swagger",
                  "swagger/{*path}"
                  );
                routes.MapRoute(
                    "handlerPath",
                    "{*path:regex(^(?!swagger))}",
                    new { controller = "Services", action = "ExecutePath" });
            });

            return app;
        }

        public static IServiceCollection UseGateWay(this IServiceCollection services)
        {
            services.AddCors();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddMvc(o =>
            {
                o.ModelBinderProviders.Insert(0, new GateWayQueryStringModelBinderProvider());
                o.ModelBinderProviders.Insert(1, new GateWayModelBinderProvider());
            }).AddJsonOptions(options =>
            {
                options.SerializerSettings.ContractResolver = new DefaultContractResolver();
            });

            return services;
        }
    }
}
