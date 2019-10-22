using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Threading.Tasks;

namespace Lamp.Client.ApiGateway.Core
{
    public class GateWayQueryStringModelBinder : IModelBinder
    {
        private readonly IModelBinder _modelBinder;
        public GateWayQueryStringModelBinder(IModelBinder modelBinder)
        {
            _modelBinder = modelBinder;
        }
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            string query = bindingContext.ActionContext.HttpContext.Request.QueryString.Value;
            GateWayQueryString queryString = new GateWayQueryString(query);
            bindingContext.ModelState.SetModelValue("query", queryString, null);
            bindingContext.Result = ModelBindingResult.Success(queryString);
            return Task.CompletedTask;
        }
    }
}
