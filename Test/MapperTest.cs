using Hake.Extension.ValueRecord;
using Hake.Extension.ValueRecord.Mapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Test
{
    public enum TestEnum
    {
        A, B, C
    }
    public class TestType
    {
        [MapProperty]
        public TestEnum EnumValue { get; set; }
        [MapProperty]
        public string StringValue { get; set; }
        [MapProperty]
        public int[] ArrayValue { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (obj is TestType t)
            {
                if (EnumValue != t.EnumValue)
                    return false;
                if (StringValue != t.StringValue)
                    return false;
                if (!ArrayValue.SequenceEqual(t.ArrayValue))
                    return false;
                return true;
            }
            else
                return false;
        }
    }

    [TestClass]
    public class MapperTest
    {
        [TestMethod]
        public void ObjectToRecordTest()
        {
            TestType value = new TestType()
            {
                EnumValue = TestEnum.B,
                StringValue = "test",
                ArrayValue = new int[] { 1, 2, 3, 4 }
            };
            RecordBase record = ObjectMapper.ToRecord(value);
            string json = Hake.Extension.ValueRecord.Json.Converter.Json(record);
            TestType retValue = ObjectMapper.ToObject<TestType>(record);
            Assert.AreEqual(true, retValue.Equals(value));
        }
    }
}
