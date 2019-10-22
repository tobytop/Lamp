using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.Logging;

namespace Lamp.Client.ApiGateway.Core
{
    public class GateWayQueryStringModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            var loggerFactory = (ILoggerFactory)context.Services.GetService(typeof(ILoggerFactory));
            IModelBinder _modelBinder = new GateWayQueryStringModelBinder(new SimpleTypeModelBinder(typeof(GateWayQueryString), loggerFactory));
            return context.Metadata.ModelType == typeof(GateWayQueryString) ? _modelBinder : null;
        }
    }
}
