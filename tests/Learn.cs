using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EContract.Dssp.Client
{
    [TestFixture]
    public class Learn
    {

        [Test]
        public void NullCheck()
        {
            Object[] array = null;

            Assert.IsTrue((array?.Length ?? 0) == 0);
        }
    }
}
