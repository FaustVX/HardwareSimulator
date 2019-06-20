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
    public class MyTestClassMuxDMux
    {
        [TestInitialize]
        public void Initialize()
        {
            Gate.RegisterGate<Nand>();
            Mux = ExternalGate.Parse(@"D:\Downloads\nand2tetris\projects\01\Mux.hdl");
            DMux = ExternalGate.Parse(@"D:\Downloads\nand2tetris\projects\01\DMux.hdl");
        }

        public Gate Mux { get; set; }
        public Gate DMux { get; set; }

        [TestMethod]
        public void MyTestMethod()
        {
            var sel = false;
            var _out = Mux.Execute(("a", true), ("b", false), ("sel", sel))["out"];

            var dmux = ExternalGate.Parse(@"D:\Downloads\nand2tetris\projects\01\DMux.hdl");
            var result = dmux.Execute(("in", _out), ("sel", sel));
            if (result["a"].HasValue)
                Assert.AreEqual(sel ? false : _out, result["a"].Value);
            else
                Assert.ThrowsException<System.InvalidOperationException>(() => result["a"].Value);
            if (result["b"].HasValue)
                Assert.AreEqual(!sel ? false : _out, result["b"].Value);
            else
                Assert.ThrowsException<System.InvalidOperationException>(() => result["b"].Value);
        }
    }
}
