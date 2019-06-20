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
            var data1 = new DataValue(value);
            var data2 = new DataValue(value);
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

        [TestMethod]
        public void GetBit()
        {
            DataValue value = 0b0000_0100;
            Assert.IsFalse(value.GetAt(1));
            Assert.IsTrue(value.GetAt(2));
            Assert.IsFalse(value.GetAt(3));

            value = DataValue.SetAt(value, 5, true);
            Assert.IsFalse(value.GetAt(4));
            Assert.IsTrue(value.GetAt(5));
            Assert.IsFalse(value.GetAt(6));

            value = DataValue.SetAt(value, 5, false);
            Assert.IsFalse(value.GetAt(4));
            Assert.IsFalse(value.GetAt(5));
            Assert.IsFalse(value.GetAt(6));
        }

        [TestMethod]
        public void Splice()
        {
            var data = new DataValue(0b0011_0101);
            var splice = data.Splice(1, 5);
            Assert.AreEqual(0b11_010, splice.Value);
            splice = data.Splice(0, 1);
            Assert.AreEqual(0b1, splice.Value);
        }
    }
}
