using Lamp.Core.Protocol.Server;

namespace Lamp.Core.Protocol.Attributes
{
    public class ServiceAttribute : ServiceDescAttribute
    {
        public ServiceAttribute()
        {
            IsWaitExecution = true;
        }

        /// <summary>
        /// serverID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 服务路径
        /// </summary>
        public string RoutePath { get; set; }

        /// <summary>
        /// http请求类型
        /// </summary>
        public string HttpMethod { get; set; }

        /// <summary>
        /// 是否可等待
        /// </summary>
        public bool IsWaitExecution { get; set; }

        /// <summary>
        ///  服务创建者
        /// </summary>
        public string CreatedBy { get; set; }

        /// <summary>
        ///  备注
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        ///  创建日期
        /// </summary>
        public string CreatedDate { get; set; }

        /// <summary>
        ///  是否需要验证
        /// </summary>
        public bool EnableAuthorization { get; set; }

        /// <summary>
        /// 角色验证
        /// </summary>
        public string Roles { get; set; }
        

        public override void Apply(ServiceDesc descriptor)
        {
            descriptor.WaitExecution = IsWaitExecution;
            descriptor.EnableAuthorization = EnableAuthorization;
            if (!string.IsNullOrEmpty(Id))
            {
                descriptor.Id = Id.ToLower();
            }

            if (!string.IsNullOrEmpty(RoutePath))
            {
                descriptor.RoutePath = RoutePath;
            }

            if (!string.IsNullOrEmpty(CreatedBy))
            {
                descriptor.CreatedBy = CreatedBy;
            }

            if (!string.IsNullOrEmpty(CreatedDate))
            {
                descriptor.CreatedDate = CreatedDate;
            }

            if (!string.IsNullOrEmpty(Comment))
            {
                descriptor.Comment = Comment;
            }

            if (!string.IsNullOrEmpty(Roles))
            {
                descriptor.Roles = Roles;
            }
            if (!string.IsNullOrEmpty(HttpMethod))
                descriptor.HttpMethod = HttpMethod;
        }
    }
}
