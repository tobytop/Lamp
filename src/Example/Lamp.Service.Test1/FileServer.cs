using Autofac;
using Lamp.Core.Filter;
using Lamp.Core.Protocol.Attributes;
using Lamp.Core.Protocol.Communication;
using Microsoft.Extensions.Configuration;

namespace Lamp.Service.Test1
{
    [ServiceVersion("v1")]
    [ServiceRoute("api/{version}")]
    public class FileServer
    {
        private IConfigurationRoot _config;
        private MyInterFace _myInterFace;
        public FileServer(Payload payload, IConfigurationRoot config, MyInterFace myInterFace)
        {
            _myInterFace = myInterFace;
            _config = config;
        }
        
        [Service(RoutePath = "/test")]
        [MyFilter]
        public string TestHealth(string message)
        {
            return _myInterFace.SendMessage(message)+ _config["config1"];
        }

        [Service(RoutePath = "/test1")]
        public string Test(string message)
        {
            return "我是谁：" + message;
        }
    }

    public interface MyInterFace
    {
        string SendMessage(string myname);
    }

    public class MyTest : MyInterFace
    {
        public string SendMessage(string myname)
        {
            return "ddddd" + myname;
        }
    }

    public class MyFilterAttribute : FilterBaseAttribute
    {
        public override void OnActionExecuting(FilterContext filterContext)
        {
            filterContext.Result = new { message = 11, test = 2222 };
        }

    }
    public class AModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<MyTest>().As<MyInterFace>();
        }
    }

}
