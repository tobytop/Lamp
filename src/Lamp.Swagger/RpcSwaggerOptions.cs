namespace Lamp.Swagger
{
    public class RpcSwaggerOptions
    {
        /// <summary>
        /// 描述 api title
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// 描述 api 版本
        /// </summary>
        public string[] Version { get; set; }

        public RpcSwaggerOptions(string title, params string[] version)
        {
            Title = title;
            Version = version;

        }

        public RpcSwaggerOptions() { }
    }
}
