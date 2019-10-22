using Autofac;
using FluentValidation;
using FluentValidation.Results;
using Lamp.Core.Protocol.Attributes;
using Lamp.Core.Protocol.Server;
using Lamp.Server.Validation;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Lamp.Service.Test
{
    [ServiceRoute("/base")]
    public class FileSever
    {
        [Service(RoutePath = "/fastdfs", HttpMethod = "Post", Comment = "测试代码")]
        [MyFilter]
        public dynamic TestHealth(TestModel testModel, TestModel1 testModel1)
        {
            return new { remessage = "已经收到：" + testModel.Num1 + "-" + testModel.Num2 + "_" + testModel1.Num11 + "-" + testModel1.Num22 };
        }

        [Service(RoutePath = "/fastdfs1", HttpMethod = "Post")]
        public Task<int> TestHealth(int myint, List<ServerFile> files)
        {
            if (files.Count > 0)
            {
                string FileName = @"d:\" + files[0].FileName;
                using (FileStream fileStream = File.Create(FileName))
                {
                    //var myb = Convert.FromBase64String(files[0].Data);
                    fileStream.Write(files[0].Data, 0, files[0].Data.Length);
                }
            }
            return Task.FromResult(myint);
        }

        public int Add(int num1, int num2)
        {
            return num1 + num2;
        }
    }

    public class TestModel
    {
        public int Num1 { get; set; }
        public int Num2 { get; set; }
    }

    public class TestValidator : AbstractValidator<TestModel>
    {
        public TestValidator()
        {
            RuleFor(o => o.Num1).Equal(0).WithMessage("测试成功了；啦啦啦");
            RuleFor(o => o.Num2).NotEqual(24).WithMessage("测试不同；大大大苏打");
        }
    }

    public class MyFilter : ValidateBaseAttribute
    {
        public override dynamic ValidateModel(ValidationFailure error)
        {
            return new
            {
                ErrorMsg = $"字段{error.PropertyName}验证错误，错误信息为：{error.ErrorMessage}",
                ErrorCode = "400"
            };
        }
    }

    public class TestModel1
    {
        public int Num11 { get; set; }
        public int Num22 { get; set; }
    }

    public class BModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<TestValidator>().As<IValidator<TestModel>>();
        }
    }
}
