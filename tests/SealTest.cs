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
        public void SealInvisibleNoProps()
        {
            DsspClient dsspClient = new DsspClient("https://www.e-contract.be/dss-ws/dss");
            dsspClient.Application.X509.Certificate = new X509Certificate2("certificate.p12", "");

            Document od;
            using (Stream i = File.OpenRead("Blank.pdf")) {
                Document id = new Document("application/pdf", i);
                od = dsspClient.Seal(id);
            }

            
            using (Stream o = File.OpenWrite("Output.pdf")) {
                od.Content.CopyTo(o);
            }
            od.Content.Seek(0, SeekOrigin.Current);

            VerifySeal(od, null, null);
        }

        [TestMethod]
        public void SealInvisibleProps()
        {
            DsspClient dsspClient = new DsspClient("https://www.e-contract.be/dss-ws/dss");
            dsspClient.Application.X509.Certificate = new X509Certificate2("certificate.p12", "");

            Document od;
            SignatureRequestProperties props = new SignatureRequestProperties()
            {
                SignerRole = "Witness",
                SignatureProductionPlace = "Iddergem"
            };
            using (Stream i = File.OpenRead("Blank.pdf"))
            {
                Document id = new Document("application/pdf", i);
                od = dsspClient.Seal(id, props);
            }


            using (Stream o = File.OpenWrite("Output.pdf"))
            {
                od.Content.CopyTo(o);
            }
            od.Content.Seek(0, SeekOrigin.Current);

            VerifySeal(od, "Witness", "Iddergem");
        }

        [TestMethod]
        public void SealVisibleProps()
        {
            DsspClient dsspClient = new DsspClient("https://www.e-contract.be/dss-ws/dss");
            dsspClient.Application.X509.Certificate = new X509Certificate2("certificate.p12", "");

            Document od;
            SignatureRequestProperties props = new SignatureRequestProperties()
            {
                SignerRole = "Gard",
                SignatureProductionPlace = "Iddergem",
                VisibleSignature = new ImageVisibleSignature()
                {
                    Page = 1,
                    X = 100,
                    Y = 100
                }
            };
            using (Stream i = File.OpenRead("Blank.pdf"))
            {
                Document id = new Document("application/pdf", i);
                od = dsspClient.Seal(id, props);
            }


            using (Stream o = File.OpenWrite("Output.pdf"))
            {
                od.Content.CopyTo(o);
            }
            od.Content.Seek(0, SeekOrigin.Current);

            VerifySeal(od, "Gard", "Iddergem");
        }

        private void VerifySeal(Document d, String role, String location)
        {
            DsspClient dsspClient = new DsspClient("https://www.e-contract.be/dss-ws/dss");
            dsspClient.Application.UT.Name = "egelke";
            dsspClient.Application.UT.Password = "egelke";
            SecurityInfo si = dsspClient.Verify(d);

            Assert.AreEqual(1, si.Signatures.Count, "Signature Count");

            //Validate signature 1
            Assert.AreEqual("SERIALNUMBER=12345, CN=Test Signing Key, C=BE", si.Signatures[0].SignerSubject, "Signature 1: SignerSubject (DSS notation)");
            Assert.AreEqual("SERIALNUMBER=12345, CN=Test Signing Key, C=BE", si.Signatures[0].Signer.Subject, "Signature 1: Signer.Subject (Windows notation)");
            Assert.IsTrue(si.Signatures[0].SigningTime > (DateTime.Now - TimeSpan.FromMinutes(5))
                && si.Signatures[0].SigningTime < (DateTime.Now + TimeSpan.FromMinutes(5)), "Signature 1: SigningTime");
            Assert.AreEqual(role, si.Signatures[0].SignerRole, "Signature 1: SignerRole");
            Assert.AreEqual(location, si.Signatures[0].SignatureProductionPlace, "Signature 1: SignatureProductionPlace");

            //Validate timestamp validity
            Assert.AreEqual(new DateTime(2020, 8, 18, 19, 11, 6, DateTimeKind.Utc), si.TimeStampValidity, "TimeStampValidity");
        }
    }
}
