using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.Logging;

namespace Lamp.Client.ApiGateway.Core
{
    public class GateWayModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            var loggerFactory = (ILoggerFactory)context.Services.GetService(typeof(ILoggerFactory));
            IModelBinder _modelBinder = new GateWayModelBinder(new SimpleTypeModelBinder(typeof(GateWayModel), loggerFactory));
            return context.Metadata.ModelType == typeof(GateWayModel) ? _modelBinder : null;
        }
    }
}
