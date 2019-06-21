using Microsoft.VisualStudio.TestTools.UnitTesting;
using HardwareSimulator.Core;
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
    public class BenEater
    {
        [TestInitialize, TestCleanup]
        public void Initialize()
        {
            Gate.ClearGates();
            Gate.RegisterGate<Nand>();
            Gate.RegisterGate<And>();
            Gate.RegisterGate<Nor>();
            Gate.RegisterGate<XNor>();
            Gate.RegisterGate<Not>();
            Gate.RegisterGate<Or>();
            Gate.RegisterGate<Xor>();
            Gate.RegisterGate<SR_Latch>();
        }

        [TestMethod]
        public void D_Latch()
        {
            var latch = ExternalGate.Parse($@"D:\Downloads\nand2tetris\perso\{nameof(D_Latch)}.hdl");
            var result = latch.Execute(("d", false), ("en", false));

            result = latch.Execute(("d", true), ("en", true));
            Assert.IsTrue(result["out"].Value);
            result = latch.Execute(("d", false), ("en", true));
            Assert.IsFalse(result["out"].Value);
            result = latch.Execute(("d", false), ("en", false));
            Assert.IsFalse(result["out"].Value);
            result = latch.Execute(("d", true), ("en", false));
            Assert.IsFalse(result["out"].Value);
            result = latch.Execute(("d", true), ("en", true));
            Assert.IsTrue(result["out"].Value);
        }

        [TestMethod]
        public void JK_FlipFlop()
        {
            var latch = ExternalGate.Parse($@"D:\Downloads\nand2tetris\perso\{nameof(JK_FlipFlop)}.hdl");
            var result = latch.Execute(("j", false), ("k", false), ("en", false));

            result = latch.Execute(("j", true), ("en", true));
            Assert.IsTrue(result["out"].Value);
            result = latch.Execute(("j", false), ("en", true));
            Assert.IsTrue(result["out"].Value);
            result = latch.Execute(("j", false), ("en", false));
            Assert.IsTrue(result["out"].Value);
            result = latch.Execute(("j", true), ("en", false));
            Assert.IsTrue(result["out"].Value);
            result = latch.Execute(("j", true), ("en", true));
            Assert.IsTrue(result["out"].Value);
        }
    }
}
