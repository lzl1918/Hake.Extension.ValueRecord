using System;
using System.Collections.Generic;
using System.Text;

namespace Hake.Extension.ValueRecord.Mapper
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class MapPropertyAttribute : Attribute
    {
        public string Name { get; }
        public bool Required { get; }
        public object DefaultValue { get; }

        public MapPropertyAttribute(string name = null, bool required = false, object defaultValue = null)
        {
            Name = name;
            Required = required;
            DefaultValue = defaultValue;
        }
    }
}
