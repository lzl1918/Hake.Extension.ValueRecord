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
        Int,
        Boolean,
        Double
    }

    [DebuggerDisplay("{_value}")]
    public sealed class ScalerRecord : RecordBase
    {
        public static Dictionary<Type, ScalerType> SUPPORTED_TYPES { get; } = new Dictionary<Type, ScalerType>()
        {
            [typeof(byte)] = ScalerType.Int,
            [typeof(char)] = ScalerType.String,
            [typeof(short)] = ScalerType.Int,
            [typeof(ushort)] = ScalerType.Int,
            [typeof(int)] = ScalerType.Int,
            [typeof(uint)] = ScalerType.Int,
            [typeof(long)] = ScalerType.Int,
            [typeof(ulong)] = ScalerType.Int,
            [typeof(string)] = ScalerType.String,
            [typeof(bool)] = ScalerType.Boolean,
            [typeof(float)] = ScalerType.Double,
            [typeof(double)] = ScalerType.Double
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

        public T ReadAs<T>() { return (T)_value; }
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
