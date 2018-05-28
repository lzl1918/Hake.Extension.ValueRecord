using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using Hake.Extension.ValueRecord;
using System.Diagnostics;
using Hake.Extension.ValueRecord.Json;

namespace Test
{
    [TestClass]
    public class RecordTest
    {
        [TestMethod]
        public void TestToJson()
        {

        }

        [TestMethod]
        public void TestFromJson()
        {
            decimal d = 1234;
            long v = (long)d;
            Stream file = File.OpenRead("data.json");
            RecordBase record = Converter.ReadJson(file);
            ScalerRecord scaler = (ScalerRecord)record.RecordFromPath("[5].abc");
            long value = scaler.ReadAs<long>();
            file.Dispose();
            string json = record.Json();
        }
    }
}
