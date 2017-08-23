using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Hake.Extension.ValueRecord.Helpers
{
    internal static class TypeHelper
    {
        public static bool CanValueBeNull(this Type type)
        {
            TypeInfo typeinfo = type.GetTypeInfo();
            if (typeinfo.IsValueType)
                return false;
            return true;
        }

        public static bool IsNullable(this Type type)
        {
            TypeInfo typeinfo = type.GetTypeInfo();
            if (typeinfo.IsGenericType == false)
                return false;

            Type genType = typeinfo.GetGenericTypeDefinition();
            if (genType.Equals(typeof(Nullable<>)) == false)
                return false;
            return true;
        }
        public static Type GetNullableParameter(this Type type)
        {
            if (type.IsNullable() == false)
                throw new NotSupportedException($"cannot get nullable parameter of type {type.Name}");

            return type.GenericTypeArguments[0];
        }
    }
}
