using Lamp.Core.Protocol.Server;
using System;

namespace Lamp.Core.Protocol.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public abstract class ServiceDescAttribute : Attribute
    {
        public abstract void Apply(ServiceDesc descriptor);
    }
}
