#define PROPERTY_PUBLIC_ONLY

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Hake.Extension.ValueRecord.Mapper
{

    public static class ObjectMapper
    {
        private static readonly Type LIST_GENERIC_TYPE = typeof(List<object>).GetGenericTypeDefinition();

        public static RecordBase ToRecord(object input)
        {
            if (input == null)
                return new ScalerRecord(null);

            Type valueType = input.GetType();
            if (valueType.IsEnum)
            {
                string typeName = $"{valueType.Namespace}.{valueType.Name}";
                string writeValue = $"{typeName}.{input}";
                return new ScalerRecord(writeValue);
            }

            if (valueType.IsPrimitive)
                return new ScalerRecord(input);

            if (valueType.Name == "String" && valueType.Namespace == "System")
                return new ScalerRecord(input);

            Type ienumType = valueType.GetInterface("System.Collections.IEnumerable");
            if (ienumType != null)
            {
                MethodInfo getEnumeratorMethod = ienumType.GetMethod("GetEnumerator");
                IEnumerator enumerator = (IEnumerator)getEnumeratorMethod.Invoke(input, null);
                ListRecord listRecord = new ListRecord();
                while (enumerator.MoveNext())
                    listRecord.Add(ToRecord(enumerator.Current));
                return listRecord;
            }

            if (valueType.IsClass)
            {
                BindingFlags propertyFlags = BindingFlags.Instance;
#if PROPERTY_PUBLIC_ONLY
                propertyFlags |= BindingFlags.Public;
#endif
                SetRecord setRecord = new SetRecord();
                PropertyInfo[] properties = valueType.GetProperties(propertyFlags);
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
                    setRecord.Add(propertyName, ToRecord(propertyValue));
                }
                return setRecord;
            }

            throw new Exception($"can not map to record of type {valueType.Namespace}.{valueType.Name}");
        }

        public static object ToObject(Type type, RecordBase record)
        {
            if (record is ScalerRecord scalerRecord)
            {
                if (type.Equals(scalerRecord.Value.GetType()))
                    return scalerRecord.Value;

                if (type.IsEnum)
                {
                    if (scalerRecord.ScalerType == ScalerType.String)
                    {
                        string enumString = (string)scalerRecord.Value;
                        int pointIndex = enumString.LastIndexOf('.');
                        string typeString = enumString.Substring(0, pointIndex);
                        string valueString = enumString.Substring(pointIndex + 1);
                        return Enum.Parse(type, valueString);
                    }
                    throw new InvalidCastException($"can not cast to enum type, string excepted but {scalerRecord.ScalerType} received");
                }
                if (type.IsPrimitive)
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
                    MethodInfo setMethodInfo = type.GetMethod("Set", BindingFlags.Instance | BindingFlags.Public);
                    object[] parameters = new object[2];
                    for (int i = 0; i < elements.Count; i++)
                    {
                        parameters[0] = i;
                        parameters[1] = elements[i];
                        setMethodInfo.Invoke(array, parameters);
                    }
                    return array;
                }

                Type ienumerableType = type.GetInterface("System.Collections.Generic.IEnumerable`1");
                if (ienumerableType != null)
                {
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

                }
            }
            else if (record is SetRecord setRecord)
            {
                if (type.IsClass)
                {
                    object instance = Activator.CreateInstance(type);
                    BindingFlags propertyFlags = BindingFlags.Instance;
#if PROPERTY_PUBLIC_ONLY
                    propertyFlags |= BindingFlags.Public;
#endif
                    PropertyInfo[] properties = type.GetProperties(propertyFlags);
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
                        else if (!mapPropertyAttribute.Required)
                        {
                            if (mapPropertyAttribute.DefaultValue == null)
                            {
                                parameter[0] = GetDefault(property.PropertyType);
                                setMethod.Invoke(instance, parameter);
                            }
                            else
                            {
                                parameter[0] = mapPropertyAttribute.DefaultValue;
                                setMethod.Invoke(instance, parameter);
                            }
                        }
                        else
                            throw new Exception($"missing property {propertyName}");
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
            if (type.IsValueType)
                return Activator.CreateInstance(type);
            else
                return null;
        }
    }
}
