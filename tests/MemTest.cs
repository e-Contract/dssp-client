using NUnit.Framework;
using System;

namespace EContract.Dssp.Client
{
    [TestFixture]
    public class MemTest
    {
        [Test]
        public void TestMethod1()
        {
            for(int i=0;i<100000; i++)
            {
                var pxy = new DsspClient("https://www.e-contract.be/dss-ws/dss");
            }
        }
    }
}
