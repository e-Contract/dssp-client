using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EContract.Dssp.Client
{
    [TestClass]
    public class Learn
    {

        [TestMethod]
        public void NullCheck()
        {
            Object[] array = null;

            Assert.IsTrue((array?.Length ?? 0) == 0);
        }
    }
}
