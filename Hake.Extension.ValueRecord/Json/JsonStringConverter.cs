using Hake.Extension.ValueRecord.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Hake.Extension.ValueRecord.Json
{
    internal static class JsonStringConverter
    {
        public static string RecordJson(RecordBase record)
        {
            return RecordJson(record, 0, true);
        }

        private static string RecordJson(RecordBase record, int indent, bool ignoreFirstIndent)
        {
            if (record is ScalerRecord)
                return ScalerJson(record as ScalerRecord, indent, ignoreFirstIndent);
            else if (record is SetRecord)
                return SetJson(record as SetRecord, indent, ignoreFirstIndent);
            else if (record is ListRecord)
                return ListJson(record as ListRecord, indent, ignoreFirstIndent);
            else
                throw new NotSupportedException($"cannot parse json of record {record.GetType().Name}");
        }
        private static string ScalerJson(ScalerRecord record, int indent, bool ignoreFirstIndent)
        {
            StringBuilder builder = new StringBuilder(capacity: 64);
            if (ignoreFirstIndent == false)
                builder.AppendIndent(indent);
            switch (record.ScalerType)
            {
                case ScalerType.Null:
                    builder.Append("null");
                    break;
                case ScalerType.String:
                    builder.AppendFormat("\"{0}\"", record.Value);
                    break;
                case ScalerType.Int:
                case ScalerType.Double:
                    builder.Append(record.Value.ToString());
                    break;
                case ScalerType.Boolean:
                    builder.Append(record.Value.ToString().ToLower());
                    break;
                default:
                    throw new NotImplementedException();
            }
            return builder.ToString();
        }
        private static string SetJson(SetRecord record, int indent, bool ignoreFirstIndent)
        {
            StringBuilder builder = new StringBuilder(capacity: record.Count * 64);
            if (ignoreFirstIndent == false)
                builder.AppendIndent(indent);
            builder.Append('{');
            int indentPlus = indent + 1;
            int indentPPlus = indentPlus + 1;
            foreach (var pair in record)
            {
                builder.AppendLine();
                builder.AppendIndent(indentPlus);
                builder.AppendFormat("\"{0}\": {1}", pair.Key, RecordJson(pair.Value, indentPlus, true));
                builder.Append(',');
            }
            if (builder.Length > 0)
                builder.Remove(builder.Length - 1, 1);
            builder.AppendLine();
            builder.AppendIndent(indent);
            builder.Append('}');
            return builder.ToString();
        }
        private static string ListJson(ListRecord record, int indent, bool ignoreFirstIndent)
        {
            StringBuilder builder = new StringBuilder(capacity: record.Count * 64);
            if (ignoreFirstIndent == false)
                builder.AppendIndent(indent);
            builder.Append('[');
            int indentPlus = indent + 1;
            if (record.All(rec => rec is ScalerRecord))
            {
                foreach (var value in record)
                {
                    builder.Append(RecordJson(value, indentPlus, true));
                    builder.Append(',');
                }
                if (record.Count > 0)
                    builder.Remove(builder.Length - 1, 1);
                builder.Append(']');
            }
            else
            {
                foreach (var value in record)
                {
                    builder.AppendLine();
                    builder.Append(RecordJson(value, indentPlus, false));
                    builder.Append(',');
                }
                if (builder.Length > 0)
                    builder.Remove(builder.Length - 1, 1);
                builder.AppendLine();
                builder.AppendIndent(indent);
                builder.Append(']');
            }
            return builder.ToString();
        }
    }
}
