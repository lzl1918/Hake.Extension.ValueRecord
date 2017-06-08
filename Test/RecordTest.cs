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
            Stream file = File.OpenRead("data.json");
            RecordBase record = Converter.ReadJson(file);
            file.Dispose();
            string json = record.Json();
            RecordBase get = record.FromPath("[5].cs.a");
        }
    }
}
