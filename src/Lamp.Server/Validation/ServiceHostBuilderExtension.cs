using Autofac;
using FluentValidation;

namespace Lamp.Server.Validation
{
    public static partial class ServiceHostBuilderExtension
    {
        public static IServiceHostServerBuilder UseValidation(this IServiceHostServerBuilder serviceHostBuilder)
        {
            serviceHostBuilder.RegisterService(containerBuilder =>
            {
                containerBuilder.RegisterType<ValidatorFactory>().As<IValidatorFactory>().SingleInstance();
            });

            return serviceHostBuilder;
        }
    }
}
