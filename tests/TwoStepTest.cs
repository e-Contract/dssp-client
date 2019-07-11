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
    public class TwoStepTest
    {
        private static X509Certificate2 Signer;

        [ClassInitialize]
        public static void ClassInit(TestContext ctx)
        {
            X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);

            X509Certificate2Collection collection = store.Certificates.Find(X509FindType.FindBySubjectName, "Bryan Brouckaert (Signature)", true);
            Signer = collection.Cast<X509Certificate2>().AsQueryable().FirstOrDefault();
        }

        [TestMethod]
        public void Sign2StepInvisibleNoProps()
        {
            DsspClient dsspClient = new DsspClient("https://www.e-contract.be/dss-ws/dss");
            dsspClient.Application.X509.Certificate = new X509Certificate2("certificate.p12", "");
            dsspClient.Signer = Signer;

            Dssp2StepSession s;
            using (Stream i = File.OpenRead("Blank.pdf")) {
                Document id = new Document("application/pdf", i);
                s = dsspClient.UploadDocumentFor2Step(id);
            }

            s.Sign();
            Document od = dsspClient.DownloadDocument(s);
            using (Stream o = File.OpenWrite("Output.pdf")) {
                od.Content.CopyTo(o);
            }
            od.Content.Seek(0, SeekOrigin.Current);

            Verify(od, null, null);
        }


        [TestMethod]
        public void Sign2StepInvisibleProps()
        {
            DsspClient dsspClient = new DsspClient("https://www.e-contract.be/dss-ws/dss");
            dsspClient.Application.X509.Certificate = new X509Certificate2("certificate.p12", "");
            dsspClient.SignerChain = new X509Certificate2[] { Signer };

            Dssp2StepSession s;
            SignatureRequestProperties props = new SignatureRequestProperties()
            {
                SignerRole = "Witness",
                SignatureProductionPlace = "Iddergem"
            };
            using (Stream i = File.OpenRead("Blank.pdf"))
            {
                Document id = new Document("application/pdf", i);
                s = dsspClient.UploadDocumentFor2Step(id, props);
            }

            s.Sign();
            Document od = dsspClient.DownloadDocument(s);
            using (Stream o = File.OpenWrite("Output.pdf"))
            {
                od.Content.CopyTo(o);
            }
            od.Content.Seek(0, SeekOrigin.Current);

            Verify(od, "Witness", "Iddergem");
        }

        private void Verify(Document d, String role, String location)
        {
            DsspClient dsspClient = new DsspClient("https://www.e-contract.be/dss-ws/dss");

            SecurityInfo si = dsspClient.Verify(d);

            Assert.AreEqual(1, si.Signatures.Count, "Signature Count");

            //Validate signature 1
            Assert.AreEqual("SERIALNUMBER=79021802145, GIVENNAME=Bryan Eduard, SURNAME=Brouckaert, CN=Bryan Brouckaert (Signature), C=BE", si.Signatures[0].SignerSubject, "Signature 1: SignerSubject (DSS notation)");
            Assert.AreEqual("SERIALNUMBER=79021802145, G=Bryan Eduard, SN=Brouckaert, CN=Bryan Brouckaert (Signature), C=BE", si.Signatures[0].Signer.Subject, "Signature 1: Signer.Subject (Windows notation)");
            Assert.IsTrue(si.Signatures[0].SigningTime > (DateTime.Now - TimeSpan.FromMinutes(5))
                && si.Signatures[0].SigningTime < (DateTime.Now + TimeSpan.FromMinutes(5)), "Signature 1: SigningTime");
            Assert.AreEqual(role, si.Signatures[0].SignerRole, "Signature 1: SignerRole");
            Assert.AreEqual(location, si.Signatures[0].SignatureProductionPlace, "Signature 1: SignatureProductionPlace");

            //Validate timestamp validity
            Assert.AreEqual(new DateTime(2020, 8, 18, 19, 11, 6, DateTimeKind.Utc), si.TimeStampValidity, "TimeStampValidity");
        }
    }
}
