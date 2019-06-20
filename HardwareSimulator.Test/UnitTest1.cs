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
    public class UnitTest1
    {
        public Nand Nand { get; set; }

        [TestInitialize]
        public void  Initialize()
        {
            Nand = new Nand();
        }

        [TestMethod]
        public void ContainsOut()
        {
            var execute = Nand.Execute(("a", true), ("b", true));
            Assert.IsTrue(execute.ContainsKey("out"));
        }

        [TestMethod]
        public void NandTrueTrueThenFalse()
        {
            var execute = Nand.Execute(("a", true), ("b", true));
            Assert.IsFalse(execute["out"].Value);
        }

        [TestMethod]
        public void NandNullNullThenNull()
        {
            var execute = Nand.Execute(("a", null), ("b", true));
            Assert.IsNull(execute["out"]);
        }

        [TestMethod]
        public void NandTrueFalseThenTrue()
        {
            var execute = Nand.Execute(("a", true), ("b", false));
            Assert.IsTrue(execute["out"].Value);
        }

        [TestMethod]
        public void NandFalseTrueThenTrue()
        {
            var execute = Nand.Execute(("a", false), ("b", true));
            Assert.IsTrue(execute["out"].Value);
        }

        [TestMethod]
        public void NandFalseFalseThenTrue()
        {
            var execute = Nand.Execute(("a", false), ("b", false));
            Assert.IsTrue(execute["out"].Value);
        }
    }
}
