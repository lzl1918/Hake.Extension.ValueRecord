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

        public static RecordBase ReadJson(string json)
        {
            StringReader reader = new StringReader(json);
            return StringJsonConverter.ReadJson(reader);
        }
        public static RecordBase ReadJson(Stream stream)
        {
            StreamReader reader = new StreamReader(stream);
            return StringJsonConverter.ReadJson(reader);
        }
    }
}
