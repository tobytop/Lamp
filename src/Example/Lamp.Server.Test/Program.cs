using Autofac;
using Lamp.Core.OAuth;
using Lamp.Core.Protocol.Server;
using Lamp.Server.HealthCheck;
using Lamp.Server.OAuth;
using Lamp.Server.Transport;
using Lamp.Server.Validation;
using System;

namespace Lamp.Server.Test
{
    internal class Program
    {
        private static void Main(string[] args)
        {

            //MonitorTest test = new MonitorTest("test");
            //test.MyMutex.WaitOne();
            //Console.Write("测试");
            //Console.ReadLine();
            ContainerBuilder containerBuilder = new ContainerBuilder();
            IServiceHostServerBuilder builder = new ServiceHostServerBuilder(containerBuilder)
               .LoadServices("Lamp.Service.Test")
               .UseRpcForTransfer(8007)
               .UseConsulCheckHealth(new RegisterServer("127.0.0.1", 8500))
               .UseValidation()
               .UseJwtForOAuth(new JwtAuthorizationOptions
               {
                   ServerIp = "127.0.0.1",
                   ServerPort = 8007,
                   SecretKey = "test",
                   ExpireTimeSpan = new TimeSpan(1, 0, 0),
                   TokenEndpointPath = "/base/token",
                   ValidateLifetime = true,
                   CheckCredential = o =>
                   {
                       if (o.UserName == "admin" && o.Password == "admin")
                       {
                           o.AddClaim("department", "IT部");
                       }
                       else
                       {
                           o.Rejected("401", "acount or password incorrect");
                       }
                   }
               });
            //.UseInServerForDiscovery();
            try
            {
                using (Core.IServiceHost host = builder.Build())
                {
                    host.Run();
                    Console.ReadLine();
                }

            }
            catch (Exception e)
            {
                Console.Write(e);
                Console.ReadLine();
            }

        }
    }
}
