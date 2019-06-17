using Microsoft.VisualStudio.TestTools.UnitTesting;
using HardwareSimulator.Core;

namespace HardwareSimulator.Test
{
    [TestClass]
    public class UnitTest2
    {
        public ExternalGate Not { get; set; }
        public ExternalGate Not16 { get; set; }
        public ExternalGate And { get; set; }
        public ExternalGate Xor { get; set; }

        [TestInitialize, TestCleanup]
        public void Initialize1()
        {
            Gate.Gates.Clear();
            Gate.RegisterGate(new Nand());
        }

        [TestMethod]
        public void TestMethodNot()
        {
            Not = ExternalGate.Parse(@"D:\Downloads\nand2tetris\projects\01\Not.hdl");
            Assert.IsNotNull(Not);
            Assert.AreEqual("Not", Not.Name, false);
        }

        [TestMethod]
        public void TestMethodNot16()
        {
            Not16 = ExternalGate.Parse(@"D:\Downloads\nand2tetris\projects\01\Not16.hdl");
            Assert.IsNotNull(Not16);
            Assert.AreEqual("Not16", Not16.Name, false);
        }

        [TestMethod]
        public void TestMethodAnd()
        {
            And = ExternalGate.Parse(@"D:\Downloads\nand2tetris\projects\01\And.hdl");
            Assert.IsNotNull(And);
            Assert.AreEqual("And", And.Name, false);
        }

        [TestMethod]
        public void TestMethodXor()
        {
            TestXor();
        }
        
        public void TestXor()
        {
            Xor = ExternalGate.Parse(@"D:\Downloads\nand2tetris\projects\01\Xor.hdl");
            Assert.IsNotNull(Xor);
            Assert.AreEqual("Xor", Xor.Name, false);
            Assert.AreEqual(5, Gate.Gates.Count);
        }

        [TestMethod]
        public void MyTestMethodXor()
        {
            TestXor();
            var result = Xor.Execute(("a", true), ("b", true));
            Assert.IsFalse(result["out"].Value);
        }

        [TestMethod]
        public void MyTestMethodNot()
        {
            TestMethodNot();
            var result = Not.Execute(("in", true));
            Assert.IsFalse(result["out"].Value);
        }
    }
}
