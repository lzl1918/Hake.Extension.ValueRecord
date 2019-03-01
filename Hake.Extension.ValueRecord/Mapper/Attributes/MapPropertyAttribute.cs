using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Hake.Extension.ValueRecord.Mapper
{
    public enum MissingAction
    {
        Throw,
        TypeDefault,
        GivenValue,
        CreateInstance,
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class MapPropertyAttribute : Attribute
    {
        public string Name { get; }
        public MissingAction MissingAction { get; }
        public object DefaultValue { get; }
        public Type ConverterType { get; }

        public MapPropertyAttribute(string name = null, MissingAction missingAction = MissingAction.Throw, object defaultValue = null, Type converterType = null)
        {
            Name = name;
            MissingAction = missingAction;
            DefaultValue = defaultValue;

            if (converterType != null && !IsValidConverterType(converterType))
                throw new Exception("not a valid converter type");
            ConverterType = converterType;
        }

        private static Type ConverterBaseType = typeof(IScalerTargetTypeConverter<string, string>).GetGenericTypeDefinition();
        private static bool IsValidConverterType(Type converterType)
        {
#if NETSTANDARD2_0 || NET452
            IEnumerable<Type> interfaces = converterType.GetInterfaces();
#else
            IEnumerable<Type> interfaces = converterType.GetTypeInfo().ImplementedInterfaces;
#endif
            foreach (Type interfaceType in interfaces)
            {
#if NETSTANDARD2_0 || NET452
                if (!interfaceType.IsGenericType)
#else
                if (interfaceType.GetTypeInfo().IsGenericType)
#endif
                    continue;
                if (!interfaceType.GetGenericTypeDefinition().Equals(ConverterBaseType))
                    continue;
#if NETSTANDARD2_0 || NET452
                Type scalerType = interfaceType.GetGenericArguments()[1];
#else
                Type scalerType = interfaceType.GenericTypeArguments[1];
#endif
                if (ScalerRecord.SUPPORTED_TYPES.ContainsKey(scalerType))
                    return true;
            }
            return false;
        }
    }
}