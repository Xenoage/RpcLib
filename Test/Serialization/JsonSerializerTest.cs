using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xenoage.RpcLib.Serialization {

    /// <summary>
    /// Tests for <see cref="JsonSerializer"/>.
    /// </summary>
    [TestClass]
    public class JsonSerializerTest {

        #region Test data

        private TestClass testObject = new TestClass {
            Number = 5,
            Child = new TestChildClass { Text = "Hello" },
            Array = new List<int> { 1, 2, 3 },
            NullValue = (int?)null,
            EnumValue = TestEnum.FlagTwo
        };
        private string json = "{'number':5,'child':{'text':'Hello'},'array':[1,2,3],'nullValue':null,'enumValue':'FlagTwo'}".Replace("'", "\"");

        #endregion

        #region Tests

        [TestMethod]
        public void Serialize_Object() {
            string actual = Encoding.UTF8.GetString(new JsonSerializer().Serialize(testObject));
            Assert.AreEqual(json, actual);
        }

        [TestMethod]
        public void Serialize_Number() {
            string actual = Encoding.UTF8.GetString(new JsonSerializer().Serialize(5.123));
            Assert.AreEqual("5.123", actual);
        }

        [TestMethod]
        public void Deserialize_Object() {
            var actual = new JsonSerializer().Deserialize<TestClass>(Encoding.UTF8.GetBytes(json));
            Assert.AreEqual(testObject, actual);
        }

        [TestMethod]
        public void Deserialize_Number() {
            double actual = new JsonSerializer().Deserialize<double>(Encoding.UTF8.GetBytes("5.123"));
            Assert.AreEqual(5.123, actual);
        }

        #endregion

        #region Test classes

        public class TestClass {
            public int Number { get; set; }
            public TestChildClass Child { get; set; }
            public IList<int> Array { get; set; }
            public int? NullValue { get; set; }
            public TestEnum EnumValue { get; set; }

            public override bool Equals(object? obj) {
                return obj is TestClass @class &&
                       Number == @class.Number &&
                       Child.Equals(@class.Child) &&
                       Array.SequenceEqual(@class.Array) &&
                       NullValue == @class.NullValue &&
                       EnumValue == @class.EnumValue;
            }
        }

        public class TestChildClass {
            public string Text { get; set; }

            public override bool Equals(object? obj) {
                return obj is TestChildClass @class &&
                       Text == @class.Text;
            }
        }

        public enum TestEnum {
            FlagOne,
            FlagTwo
        }

        #endregion

    }

}
