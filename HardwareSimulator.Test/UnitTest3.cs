using HardwareSimulator.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HardwareSimulator.Test
{
    [TestClass]
    public class UnitTest3
    {
        [TestMethod]
        public void ContainsOut()
        {
            byte upper = 0xf0;
            byte lower = 0x00;
            var value = (ushort)((upper << 8) + lower);
            var data1 = new DataValue(upper, lower);
            var data2 = new DataValue(upper, lower);
            Assert.AreEqual(upper, data1.UpperByte);
            Assert.AreEqual(lower, data1.LowerByte);
            Assert.AreEqual(value, data1.Value);
            Assert.IsTrue(data1.UpperBool);
            Assert.IsFalse(data1.LowerBool);
            Assert.IsTrue(data1);
            Assert.IsTrue(data1 == data2);
        }

        [TestMethod]
        public void DataValueTrue()
        {
            DataValue data = true;
            Assert.IsTrue(data);
            Assert.AreEqual(0xff, data.LowerByte);
            Assert.AreEqual(0xff, data.UpperByte);
        }

        [TestMethod]
        public void DataValueFalse()
        {
            DataValue data = false;
            Assert.IsFalse(data);
            Assert.AreEqual(0x00, data.LowerByte);
            Assert.AreEqual(0x00, data.UpperByte);
            data++;
            Assert.IsTrue(data);
            Assert.AreEqual(0x01, data.LowerByte);
            Assert.AreEqual(0x00, data.UpperByte);
        }
    }
}
