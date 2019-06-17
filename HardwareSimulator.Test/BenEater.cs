using Microsoft.VisualStudio.TestTools.UnitTesting;
using HardwareSimulator.Core;

namespace HardwareSimulator.Test
{
    [TestClass]
    public class BenEater
    {
        private ExternalGate _latch;

        [TestInitialize, TestCleanup]
        public void Initialize()
        {
            Gate.Gates.Clear();
            Gate.RegisterGate<Nand>();
            Gate.RegisterGate<And>();
            Gate.RegisterGate<Nor>();
            Gate.RegisterGate<XNor>();
            Gate.RegisterGate<Not>();
            Gate.RegisterGate<Or>();
            Gate.RegisterGate<Xor>();
            _latch = ExternalGate.Parse(@"D:\Downloads\nand2tetris\perso\D_Latch.hdl");
        }

        [TestMethod]
        public void MyTestMethod1()
        {
            var result = _latch.Execute(("d", true), ("en", false));
                result = _latch.Execute(("d", true), ("en", true));
            Assert.IsTrue(result["out"].Value);
                result = _latch.Execute(("d", false), ("en", true));
            Assert.IsFalse(result["out"].Value);
        }
    }
}
