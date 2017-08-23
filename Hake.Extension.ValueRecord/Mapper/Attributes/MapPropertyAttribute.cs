using System;
using System.Collections.Generic;
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

        public MapPropertyAttribute(string name = null, MissingAction missingAction = MissingAction.Throw, object defaultValue = null)
        {
            Name = name;
            MissingAction = missingAction;
            DefaultValue = defaultValue;
        }
    }
}
