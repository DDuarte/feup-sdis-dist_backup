using System;
using DBS;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DBSTests
{
    [TestClass]
    public class StringValueAttributeTests
    {
        enum TestEnum
        {
            [StringValue("Test1")]
            Value1,
            [StringValue("")]
            Value2,
            [StringValue(null)]
            Value3,
            Value4
        }

        [TestMethod]
        public void TestGetString()
        {
            Assert.AreEqual("Test1", StringValueAttribute.Get(TestEnum.Value1));
            Assert.AreEqual("", StringValueAttribute.Get(TestEnum.Value2));
            Assert.AreEqual(null, StringValueAttribute.Get(TestEnum.Value3));
            Assert.AreEqual(null, StringValueAttribute.Get(TestEnum.Value4));
        }

        [TestMethod]
        public void TestGetEnum()
        {
            Assert.AreEqual(TestEnum.Value1, StringValueAttribute.Get<TestEnum>("Test1"));
            Assert.AreEqual(TestEnum.Value2, StringValueAttribute.Get<TestEnum>(""));
            Assert.AreEqual(TestEnum.Value3, StringValueAttribute.Get<TestEnum>(null));
            Assert.AreEqual(TestEnum.Value1, StringValueAttribute.Get<TestEnum>("Missing")); // default(0) is Value1
        }
    }
}
