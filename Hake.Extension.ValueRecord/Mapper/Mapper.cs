#define PROPERTY_PUBLIC_ONLY

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Hake.Extension.ValueRecord.Mapper
{

    public static class ObjectMapper
    {
        private static readonly Type LIST_GENERIC_TYPE = typeof(List<object>).GetGenericTypeDefinition();

        public static RecordBase ToRecord(object input, bool ignoreKeyCase = false)
        {
            if (input == null)
                return new ScalerRecord(null);

            Type valueType = input.GetType();
#if NETSTANDARD1_2
            TypeInfo valueTypeInfo = valueType.GetTypeInfo();
#endif

#if NETSTANDARD2_0 || NET452
            if (valueType.IsEnum)
#else
            if (valueTypeInfo.IsEnum)
#endif
            {
                string typeName = $"{valueType.Namespace}.{valueType.Name}";
                string writeValue = $"{typeName}.{input}";
                return new ScalerRecord(writeValue);
            }

#if NETSTANDARD2_0 || NET452
            if (valueType.IsPrimitive)
#else
            if (valueTypeInfo.IsPrimitive)
#endif
                return new ScalerRecord(input);

            if (valueType.Name == "String" && valueType.Namespace == "System")
                return new ScalerRecord(input);

#if NETSTANDARD2_0 || NET452
            Type ienumType = valueType.GetInterface("System.Collections.IEnumerable");
#else
            Type ienumType = valueTypeInfo.ImplementedInterfaces.FirstOrDefault(t => t.FullName == "System.Collections.IEnumerable");
            TypeInfo ienumTypeInfo = ienumType == null ? null : ienumType.GetTypeInfo();
#endif
            if (ienumType != null)
            {
#if NETSTANDARD2_0 || NET452
                MethodInfo getEnumeratorMethod = ienumType.GetMethod("GetEnumerator");
#else
                MethodInfo getEnumeratorMethod = ienumTypeInfo.DeclaredMethods.FirstOrDefault(m => m.Name == "GetEnumerator");
#endif
                IEnumerator enumerator = (IEnumerator)getEnumeratorMethod.Invoke(input, null);
                ListRecord listRecord = new ListRecord();
                while (enumerator.MoveNext())
                    listRecord.Add(ToRecord(enumerator.Current, ignoreKeyCase));
                return listRecord;
            }

#if NETSTANDARD2_0 || NET452
            if (valueType.IsClass)
#else
            if (valueTypeInfo.IsClass)
#endif
            {
#if NETSTANDARD2_0 || NET452
                BindingFlags propertyFlags = BindingFlags.Instance;
#if PROPERTY_PUBLIC_ONLY
                propertyFlags |= BindingFlags.Public;
                PropertyInfo[] properties = valueType.GetProperties(propertyFlags);
#endif
#else
                PropertyInfo[] properties = valueTypeInfo.DeclaredProperties.ToArray();
#endif
                SetRecord setRecord = new SetRecord(ignoreKeyCase);
                MapPropertyAttribute mapPropertyAttribute;
                MethodInfo getMethod;
                string propertyName;
                object propertyValue;
                foreach (PropertyInfo property in properties)
                {
                    getMethod = property.GetMethod;
                    if (getMethod == null)
                        continue;
                    mapPropertyAttribute = property.GetCustomAttribute<MapPropertyAttribute>();
                    if (mapPropertyAttribute == null)
                        continue;
                    propertyName = GetNameOrDefault(property, mapPropertyAttribute);
                    propertyValue = getMethod.Invoke(input, null);
                    setRecord.Add(propertyName, ToRecord(propertyValue, ignoreKeyCase));
                }
                return setRecord;
            }

            throw new Exception($"can not map to record of type {valueType.Namespace}.{valueType.Name}");
        }

        public static object ToObject(Type type, RecordBase record)
        {
#if NETSTANDARD1_2
            TypeInfo typeInfo = type.GetTypeInfo();
#endif
            if (record is ScalerRecord scalerRecord)
            {
                if (scalerRecord.Value != null && type.Equals(scalerRecord.Value.GetType()))
                    return scalerRecord.Value;
#if NETSTANDARD2_0 || NET452
                if (scalerRecord.Value == null && !type.IsValueType)
#else
                if (scalerRecord.Value == null && !typeInfo.IsValueType)
#endif
                    return null;

#if NETSTANDARD2_0 || NET452
                if (type.IsEnum)
#else
                if (typeInfo.IsEnum)
#endif
                {
                    if (scalerRecord.ScalerType == ScalerType.String)
                    {
                        string enumString = (string)scalerRecord.Value;
                        int pointIndex = enumString.LastIndexOf('.');
                        if (pointIndex < 0)
                        {
                            try
                            {
                                return Enum.Parse(type, enumString);
                            }
                            catch (ArgumentException)
                            {
                                string[] names = Enum.GetNames(type);
                                foreach (string name in names)
                                {
                                    if (name.Equals(enumString, StringComparison.OrdinalIgnoreCase))
                                    {
                                        return Enum.Parse(type, name);
                                    }
                                }
                                throw;
                            }
                        }
                        else
                        {
                            string typeString = enumString.Substring(0, pointIndex);
                            string valueString = enumString.Substring(pointIndex + 1);
                            try
                            {
                                return Enum.Parse(type, valueString);
                            }
                            catch (ArgumentException)
                            {
                                string[] names = Enum.GetNames(type);
                                foreach (string name in names)
                                {
                                    if (name.Equals(valueString, StringComparison.OrdinalIgnoreCase))
                                    {
                                        return Enum.Parse(type, name);
                                    }
                                }
                                throw;
                            }
                        }
                    }
                    throw new InvalidCastException($"can not cast to enum type, string excepted but {scalerRecord.ScalerType} received");
                }

#if NETSTANDARD2_0 || NET452
                if (type.IsPrimitive)
#else
                if (typeInfo.IsPrimitive)
#endif
                    return Convert.ChangeType(scalerRecord.Value, type);
                if (type.Name == "String" && type.Namespace == "System" && scalerRecord.ScalerType == ScalerType.String)
                    return scalerRecord.Value;
            }
            else if (record is ListRecord listRecord)
            {
                if (type.IsArray)
                {
                    Type elementType = type.GetElementType();
                    List<object> elements = new List<object>();
                    foreach (RecordBase re in listRecord)
                        elements.Add(ToObject(elementType, re));
                    object array = Activator.CreateInstance(type, elements.Count);
#if NETSTANDARD2_0 || NET452
                    MethodInfo setMethodInfo = type.GetMethod("Set", BindingFlags.Instance | BindingFlags.Public);
#else
                    MethodInfo setMethodInfo = typeInfo.GetDeclaredMethod("Set");
#endif
                    object[] parameters = new object[2];
                    for (int i = 0; i < elements.Count; i++)
                    {
                        parameters[0] = i;
                        parameters[1] = elements[i];
                        setMethodInfo.Invoke(array, parameters);
                    }
                    return array;
                }
#if NETSTANDARD2_0 || NET452
                Type ienumerableType = type.GetInterface("System.Collections.Generic.IEnumerable`1");
#else
                Type ienumerableType = typeInfo.ImplementedInterfaces.FirstOrDefault(t => t.FullName == "System.Collections.Generic.IEnumerable`1");
                TypeInfo ienumerableTypeInfo = ienumerableType == null ? null : ienumerableType.GetTypeInfo();
#endif
                if (ienumerableType != null)
                {
#if NETSTANDARD2_0 || NET452
                    Type elementType = ienumerableType.GetGenericArguments()[0];
                    Type listType = LIST_GENERIC_TYPE.MakeGenericType(elementType);
                    if (type.IsAssignableFrom(listType))
                    {
                        object list = Activator.CreateInstance(listType);
                        MethodInfo addMethod = listType.GetMethod("Add", BindingFlags.Public | BindingFlags.Instance);
                        object[] parameters = new object[1];
                        foreach (RecordBase re in listRecord)
                        {
                            parameters[0] = ToObject(elementType, re);
                            addMethod.Invoke(list, parameters);
                        }
                        return list;
                    }
#else
                    Type elementType = ienumerableTypeInfo.GenericTypeParameters[0];
                    Type listType = LIST_GENERIC_TYPE.MakeGenericType(elementType);
                    TypeInfo listTypeInfo = listType.GetTypeInfo();
                    if (typeInfo.IsAssignableFrom(listTypeInfo))
                    {
                        object list = Activator.CreateInstance(listType);
                        MethodInfo addMethod = listTypeInfo.GetDeclaredMethod("Add");
                        object[] parameters = new object[1];
                        foreach (RecordBase re in listRecord)
                        {
                            parameters[0] = ToObject(elementType, re);
                            addMethod.Invoke(list, parameters);
                        }
                        return list;
                    }
#endif
                }
            }
            else if (record is SetRecord setRecord)
            {
#if NETSTANDARD2_0 || NET452
                if (type.IsClass)
#else
                if (typeInfo.IsClass)
#endif
                {
                    object instance = Activator.CreateInstance(type);
#if NETSTANDARD2_0 || NET452
                    BindingFlags propertyFlags = BindingFlags.Instance;
#if PROPERTY_PUBLIC_ONLY
                    propertyFlags |= BindingFlags.Public;
#endif
                    PropertyInfo[] properties = type.GetProperties(propertyFlags);
#else
                    PropertyInfo[] properties = typeInfo.DeclaredProperties.ToArray();
#endif
                    MapPropertyAttribute mapPropertyAttribute;
                    MethodInfo setMethod;
                    string propertyName;
                    RecordBase valueRecord;
                    object[] parameter = new object[1];
                    foreach (PropertyInfo property in properties)
                    {
                        setMethod = property.SetMethod;
                        if (setMethod == null)
                            continue;
                        mapPropertyAttribute = property.GetCustomAttribute<MapPropertyAttribute>();
                        if (mapPropertyAttribute == null)
                            continue;
                        propertyName = GetNameOrDefault(property, mapPropertyAttribute);
                        if (setRecord.TryGetValue(propertyName, out valueRecord))
                        {
                            parameter[0] = ToObject(property.PropertyType, valueRecord);
                            setMethod.Invoke(instance, parameter);
                        }
                        else
                        {
                            switch (mapPropertyAttribute.MissingAction)
                            {
                                case MissingAction.TypeDefault:
                                    parameter[0] = GetDefault(property.PropertyType);
                                    break;
                                case MissingAction.GivenValue:
                                    parameter[0] = mapPropertyAttribute.DefaultValue;
                                    break;
                                case MissingAction.CreateInstance:
                                    parameter[0] = Activator.CreateInstance(property.PropertyType);
                                    break;
                                case MissingAction.Throw:
                                default:
                                    throw new Exception($"missing property {propertyName}");
                            }
                            setMethod.Invoke(instance, parameter);
                        }
                    }
                    return instance;
                }
            }
            throw new InvalidCastException($"can not cast to type {type.Name}");
        }
        public static T ToObject<T>(RecordBase record)
        {
            return (T)ToObject(typeof(T), record);
        }

        private static string GetNameOrDefault(PropertyInfo property, MapPropertyAttribute attribute)
        {
            if (attribute.Name != null)
                return attribute.Name;
            string name = property.Name;
            char lowerFirstChar = ToLower(name[0]);
            return $"{lowerFirstChar}{name.Substring(1)}";
        }
        private static char ToLower(char ch)
        {
            if (ch >= 'A' && ch <= 'Z') { return (char)(ch - 'A' + 'a'); }
            return ch;
        }
        private static object GetDefault(Type type)
        {
#if NETSTANDARD2_0
            if (type.IsValueType)
#else
            if (type.GetTypeInfo().IsValueType)
#endif
                return Activator.CreateInstance(type);
            else
                return null;
        }
    }
}
