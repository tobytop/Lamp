using Newtonsoft.Json;
using System;
using System.Text;

namespace Lamp.Core.Serializer
{
    public class JasonSerializer : ISerializer
    {
        static JasonSerializer()
        {
            InitSerializer();
            InitDeserializer();
        }

        public TResult Deserialize<T, TResult>(T data) where TResult : class
        {
            return DeserializeProxy<T>.DeserializeFunc(data, typeof(TResult)) as TResult;
        }

        public object Deserialize<T>(T data, Type type)
        {
            return DeserializeProxy<T>.DeserializeFunc(data, type);
        }

        public object Deserialize(string data, Type type)
        {
            return DeserializeProxy<string>.DeserializeFunc(data, type);
        }

        public T Serialize<T>(object instance)
        {
            return SerializerProxy<T>.SerializeFunc(instance);
        }

        /// <summary>
        /// 初始化序列化代理
        /// </summary>
        private static void InitSerializer()
        {
            SerializerProxy<string>.SerializeFunc = JsonConvert.SerializeObject;
            //Rpc服务的转换
            SerializerProxy<byte[]>.SerializeFunc = instance => Encoding.UTF8.GetBytes(SerializerProxy<string>.SerializeFunc(instance));
        }

        /// <summary>
        /// 初始化反序列化代理
        /// </summary>
        private static void InitDeserializer()
        {
            DeserializeProxy<string>.DeserializeFunc = JsonConvert.DeserializeObject;

            DeserializeProxy<object>.DeserializeFunc = (content, type) => DeserializeProxy<string>.DeserializeFunc(content.ToString(), type);

            DeserializeProxy<byte[]>.DeserializeFunc = (content, type) => DeserializeProxy<string>.DeserializeFunc(Encoding.UTF8.GetString(content), type);
        }

        private static class SerializerProxy<T>
        {
            public static Func<object, T> SerializeFunc;
        }

        private static class DeserializeProxy<T>
        {
            public static Func<T, Type, object> DeserializeFunc;
        }
    }
}
