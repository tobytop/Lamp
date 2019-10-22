using Autofac;
using System;

namespace Lamp.Core.Filter
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public abstract class FilterBaseAttribute : Attribute, IFilterHandler
    {
        public IContainer Container { get; set; }

        public virtual void OnActionExecuted(FilterContext filterContext)
        {

        }
        public virtual void OnActionExecuting(FilterContext filterContext)
        {
        }
    }
}
