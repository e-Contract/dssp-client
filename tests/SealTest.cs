using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace EContract.Dssp.Client
{
    [TestClass]
    public class SealTest
    {
        [TestMethod]
        public void Basic()
        {
            DsspClient dsspClient = new DsspClient("https://www.e-contract.be/dss-ws/dss");
            dsspClient.Application.X509.Certificate = new X509Certificate2("certificate.p12", "");

            Document od;
            using (Stream i = File.OpenRead("Signed.pdf")) {
                Document id = new Document("application/pdf", i);
                od = dsspClient.Seal(id);
            }

            using (Stream o = File.OpenWrite("SignedSealed.pdf")) {
                od.Content.CopyTo(o);
            }
        }
    }
}
