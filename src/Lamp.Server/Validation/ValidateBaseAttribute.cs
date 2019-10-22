using Autofac;
using FluentValidation;
using FluentValidation.Results;
using Lamp.Core.Filter;
using Lamp.Core.Protocol.Server;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lamp.Server.Validation
{
    public abstract class ValidateBaseAttribute : FilterBaseAttribute
    {
        public abstract dynamic ValidateModel(ValidationFailure error);

        public sealed override void OnActionExecuting(FilterContext filterContext)
        {
            IValidatorFactory factory = Container.Resolve<IValidatorFactory>();

            foreach (KeyValuePair<Type, object> p in filterContext.ServiceArguments)
            {
                Type type = p.Key;

                if (type.IsClass && !type.FullName.StartsWith("System.") && typeof(List<ServerFile>) != type)
                {
                    IValidator validator = factory.GetValidator(type);
                    if (validator != null)
                    {
                        if (p.Value != null)
                        {
                            ValidationResult result = validator.Validate(p.Value);
                            if (!result.IsValid)
                            {
                                ValidationFailure error = result.Errors.First();
                                filterContext.Result = ValidateModel(error);
                            }
                        }
                    }
                }
            }
        }
    }
}
