using System;

namespace Lamp.Core.Common.TypeConverter
{
    public interface ITypeConvertProvider
    {
        object Convert(object instance, Type destinationType);
    }
}
