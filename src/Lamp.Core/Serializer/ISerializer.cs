using System;

namespace Lamp.Core.Serializer
{
    public interface ISerializer
    {
        T Serialize<T>(object instance);

        object Deserialize(string data, Type type);

        TResult Deserialize<T, TResult>(T data) where TResult : class;

        object Deserialize<T>(T data, Type type);
    }
}
