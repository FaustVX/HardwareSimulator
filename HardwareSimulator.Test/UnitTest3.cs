using Microsoft.VisualStudio.TestTools.UnitTesting;
#if Computer8Bits
using DataValue = HardwareSimulator.Core.DataValue8Bits;
using InnerType = System.Byte;
#elif Computer16Bits
using DataValue = HardwareSimulator.Core.DataValue16Bits;
using InnerType = System.UInt16;
#endif

namespace HardwareSimulator.Test
{
    [TestClass]
    public class UnitTest3
    {
        [TestMethod]
        public void ContainsOut()
        {
#if Computer8Bits
            InnerType value = 0xf0;
#elif Computer16Bits
            byte upper = 0xf0;
            byte lower = 0x00;
            var value = (InnerType)((upper << 8) + lower);
#endif
            var data1 = new DataValue(value);
            var data2 = new DataValue(value);
#if Computer16Bits
            Assert.AreEqual(upper, data1.Upper);
            Assert.AreEqual(lower, data1.Lower);
            Assert.IsTrue(data1.Upper.Bool);
            Assert.IsFalse(data1.Lower.Bool);
#endif
            Assert.AreEqual(value, data1.Value);
            Assert.IsTrue(data1);
            Assert.IsTrue(data1 == data2);
        }

        [TestMethod]
        public void DataValueTrue()
        {
            DataValue data = true;
            Assert.IsTrue(data);
#if Computer8Bits
            Assert.AreEqual(0xff, data.Value); 
#elif Computer16Bits
            Assert.AreEqual(0xff, data.Lower);
            Assert.AreEqual(0xff, data.Upper); 
#endif
        }

        [TestMethod]
        public void DataValueFalse()
        {
            DataValue data = false;
            Assert.IsFalse(data);
#if Computer8Bits
            Assert.AreEqual(0x00, data.Value);
#elif Computer16Bits
            Assert.AreEqual(0x00, data.Lower);
            Assert.AreEqual(0x00, data.Upper); 
#endif
            data++;
            Assert.IsTrue(data);
#if Computer8Bits
            Assert.AreEqual(0x01, data.Value);
#elif Computer16Bits
            Assert.AreEqual(0x01, data.Lower);
            Assert.AreEqual(0x00, data.Upper); 
#endif
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
