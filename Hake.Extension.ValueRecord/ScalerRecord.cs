using System;
using System.Collections.Generic;
using System.Text;
using Hake.Extension.ValueRecord.Internal.Helpers;
using System.Diagnostics;

namespace Hake.Extension.ValueRecord
{
    public enum ScalerType
    {
        Null,
        String,
        Boolean,
        Double,
        Decimal,
    }

    [DebuggerDisplay("{_value}")]
    public sealed class ScalerRecord : RecordBase
    {
        public static Dictionary<Type, ScalerType> SUPPORTED_TYPES { get; } = new Dictionary<Type, ScalerType>()
        {
            [typeof(byte)] = ScalerType.Decimal,
            [typeof(char)] = ScalerType.String,
            [typeof(short)] = ScalerType.Decimal,
            [typeof(ushort)] = ScalerType.Decimal,
            [typeof(int)] = ScalerType.Decimal,
            [typeof(uint)] = ScalerType.Decimal,
            [typeof(long)] = ScalerType.Decimal,
            [typeof(ulong)] = ScalerType.Decimal,
            [typeof(string)] = ScalerType.String,
            [typeof(bool)] = ScalerType.Boolean,
            [typeof(float)] = ScalerType.Double,
            [typeof(double)] = ScalerType.Double,
            [typeof(decimal)] = ScalerType.Decimal
        };

        private object _value = null;
        public object Value
        {
            get { return _value; }
            set { UpdateValue(value); }
        }
        public ScalerType ScalerType { get; private set; }

        public ScalerRecord(object value) : base(RecordType.Scaler)
        {
            UpdateValue(value);
        }

        private void UpdateValue(object value)
        {
            if (value == null)
            {
                ScalerType = ScalerType.Null;
                _value = null;
                return;
            }

            Type type = value.GetType();
            if (type.IsNullable() == true)
                type = type.GetNullableParameter();
            ScalerType scalerType;
            if (SUPPORTED_TYPES.TryGetValue(type, out scalerType) == false)
                throw new NotSupportedException($"cannot set value of type {type.Name} for ScalerRecord");

            if (type.Equals(typeof(char)))
                value = $"{value}";

            ScalerType = scalerType;
            _value = value;
        }

        public T ReadAs<T>()
        {
            if (ScalerType == ScalerType.Decimal)
            {
                Type tType = typeof(T);
                decimal value = (decimal)_value;
                object result;
                if (tType.Equals(typeof(byte)))
                    result = (byte)value;
                else if (tType.Equals(typeof(short)))
                    result = (short)value;
                else if (tType.Equals(typeof(ushort)))
                    result = (ushort)value;
                else if (tType.Equals(typeof(int)))
                    result = (int)value;
                else if (tType.Equals(typeof(uint)))
                    result = (uint)value;
                else if (tType.Equals(typeof(long)))
                    result = (long)value;
                else if (tType.Equals(typeof(ulong)))
                    result = (ulong)value;
                else
                    return (T)_value;
                return (T)result;
            }
            return (T)_value;
        }
        public bool TryReadAs<T>(out T value)
        {
            try
            {
                value = ReadAs<T>();
                return true;
            }
            catch
            {
                value = default(T);
                return false;
            }
        }
    }
}
