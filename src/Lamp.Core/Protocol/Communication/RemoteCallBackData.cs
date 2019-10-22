using System;
using System.Text;

namespace Lamp.Core.Protocol.Communication
{
    /// <summary>
    /// 回调的信息
    /// </summary>
    [Serializable]
    public class RemoteCallBackData
    {
        /// <summary>
        /// 异常信息
        /// </summary>
        public string ExceptionMessage { get; set; }

        /// <summary>
        /// 错误消息
        /// </summary>
        public string ErrorMsg { get; set; }

        /// <summary>
        /// 错误代码
        /// </summary>
        public string ErrorCode { get; set; }

        /// <summary>
        /// 结果
        /// </summary>
        public object Result { get; set; }

        /// <summary>
        /// 结果类型
        /// </summary>
        public string ResultType { get; set; }

        public bool HasError => !(string.IsNullOrEmpty(ExceptionMessage)
                                  && string.IsNullOrEmpty(ErrorCode)
                                  && string.IsNullOrEmpty(ErrorMsg));

        public string ToErrorString()
        {
            StringBuilder errorMsg = new StringBuilder();
            errorMsg.Append(!string.IsNullOrEmpty(ErrorCode) ? $"{ErrorCode}," : "");
            errorMsg.Append(!string.IsNullOrEmpty(ErrorMsg) ? $"{ErrorMsg}," : "");
            errorMsg.Append(!string.IsNullOrEmpty(ExceptionMessage) ? $"{ExceptionMessage}," : "");
            return errorMsg.ToString().TrimEnd(',');
        }
    }
}
