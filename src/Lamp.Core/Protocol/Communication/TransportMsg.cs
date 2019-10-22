using Lamp.Core.Serializer;
using System;

namespace Lamp.Core.Protocol.Communication
{
    /// <summary>
    /// Rpc传递消息的格式
    /// </summary>
    [Serializable]
    public class TransportMsg
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");

        public string ContentType { get; set; }

        public object Content { get; set; }

        /// <summary>
        /// 消息内容转换
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetContent<T>(ISerializer serializer)
        {
            try
            {
                return (T)Content;
            }
            catch (Exception)
            {
                return serializer.Serialize<T>(Content);
            }

        }
    }
}
