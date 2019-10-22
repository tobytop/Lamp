using Autofac;
using Lamp.Core.Protocol.Server;
using Lamp.Server.Config;
using Lamp.Server.HealthCheck;
using Lamp.Server.Transport;
using Lamp.Server.Validation;
using System;

namespace Lamp.Server.Test1
{
    internal class Program
    {
        //public static Mutex mutex = new Mutex();
        private static void Main(string[] args)
        {
            //MonitorTest test = new MonitorTest("test1");
            //test.MyMutex.WaitOne();
            //Console.Write(AppDomain.CurrentDomain.BaseDirectory);
            //Console.ReadLine();
            ContainerBuilder containerBuilder = new ContainerBuilder();
            IServiceHostServerBuilder builder = new ServiceHostServerBuilder(containerBuilder)
               .LoadServices("Lamp.Service.Test", "Lamp.Service.Test1")
               .UseRpcForTransfer(8008)
               .UseConsulCheckHealth(new RegisterServer("127.0.0.1", 8500))
               .LoadConifg("appsettings.json").UseValidation();
            //.UseInServerForDiscovery();

            using (Core.IServiceHost host = builder.Build())
            {
                host.Run();
                Console.ReadLine();
            }
        }
    }
}
