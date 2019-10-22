using System;

namespace Lamp.Core.Common.Logger
{
    public interface ILogger
    {
        /// <summary>
        ///   输出测试详情
        /// </summary>
        /// <param name="info"></param>
        void Debug(string info);
        /// <summary>
        ///    输出普通详情
        /// </summary>
        /// <param name="info"></param>
        void Info(string info);

        /// <summary>
        ///   输出警告
        /// </summary>
        /// <param name="info"></param>
        void Warn(string info);

        /// <summary>
        ///  输出错误详情
        /// </summary>
        /// <param name="info"></param>
        /// <param name="ex"></param>
        void Error(string info, Exception ex);
    }
}
