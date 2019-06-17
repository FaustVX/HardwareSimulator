using Microsoft.VisualStudio.TestTools.UnitTesting;
using HardwareSimulator.Core;

namespace HardwareSimulator.Test
{
    [TestClass]
    public class UnitTest2
    {
        public ExternalGate Not { get; set; }
        public ExternalGate And { get; set; }
        public ExternalGate ALU { get; set; }

        [TestInitialize]
        public void Initialize1()
        {
            Gate.RegisterGate(new Nand());
            Not = ExternalGate.Parse(@"D:\Downloads\nand2tetris\projects\01\Not.hdl");
            And = ExternalGate.Parse(@"D:\Downloads\nand2tetris\projects\01\And.hdl");
            //ALU = ExternalGate.Parse(@"D:\Downloads\nand2tetris\projects\02\ALU.hdl");
        }

        [TestMethod]
        public void TestMethod1()
        {
            Assert.IsNotNull(Not);
            Assert.AreEqual(Not.Name, "Not", false);
        }

        [TestMethod]
        public void TestMethod2()
        {
            Assert.IsNotNull(And);
            Assert.AreEqual(And.Name, "And", false);
        }
    }
}
