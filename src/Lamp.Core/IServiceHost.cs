using Autofac;
using System;

namespace Lamp.Core
{
    public interface IServiceHost : IDisposable
    {
        IContainer Container { get; }

        void DisposeAction(Action<IContainer> action);

        void RunAction(Action<IContainer> action);

        void Run();
    }
}
