using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Hake.Extension.ValueRecord.Json
{
    public static class Converter
    {
        public static string Json(this RecordBase record)
        {
            return JsonStringConverter.Json(record);
        }

        public static RecordBase ReadJson(string json, bool ignoreKeyCase = false)
        {
            StringReader reader = new StringReader(json);
            return StringJsonConverter.ReadJson(reader, ignoreKeyCase);
        }
        public static RecordBase ReadJson(Stream stream, bool ignoreKeyCase = false)
        {
            StreamReader reader = new StreamReader(stream);
            return StringJsonConverter.ReadJson(reader, ignoreKeyCase);
        }
    }
}
