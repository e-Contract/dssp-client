using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EContract.Dssp.Client
{
    [TestClass]
    public class MemTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            for(int i=0;i<100000; i++)
            {
                var pxy = new DsspClient("https://www.e-contract.be/dss-ws/dss");
            }
        }
    }
}
