namespace Lamp.Core.Filter
{
    public interface IFilterHandler
    {
        void OnActionExecuted(FilterContext filterContext);

        void OnActionExecuting(FilterContext filterContext);
    }
}
