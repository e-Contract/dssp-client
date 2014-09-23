using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Globalization;

namespace EContract.Dssp.Client
{
    [TestClass]
    public class VerifyTest
    {
        [TestMethod]
        public void AnomClientVerifyNoSignSync()
        {
            DsspClient dsspClient = new DsspClient("https://www.e-contract.be/dss-ws/dss");

            Document d = new Document("application/pdf", File.OpenRead("Blank.pdf"));
            SecurityInfo si = dsspClient.Verify(d);

            //Validate lack of signature
            Assert.IsNull(si, "Verify must return null");
        }

        [TestMethod]
        public void AnomClientVerifySingleSignSync()
        {
            DsspClient dsspClient = new DsspClient("https://www.e-contract.be/dss-ws/dss");

            Document d = new Document("application/pdf", File.OpenRead("Signed.pdf"));
            SecurityInfo si = dsspClient.Verify(d);

            Assert.AreEqual(1, si.Signatures.Count, "Signature Count");

            //Validate signature 1
            Assert.AreEqual("SERIALNUMBER=79021802145, GIVENNAME=Bryan Eduard, SURNAME=Brouckaert, CN=Bryan Brouckaert (Signature), C=BE", si.Signatures[0].SignerSubject, "Signature 1: SignerSubject (DSS notation)");
            Assert.AreEqual("SERIALNUMBER=79021802145, G=Bryan Eduard, SN=Brouckaert, CN=Bryan Brouckaert (Signature), C=BE", si.Signatures[0].Signer.Subject, "Signature 1: Signer.Subject (Windows notation)");
            Assert.AreEqual(new DateTime(2014, 9, 23, 20, 11, 34, DateTimeKind.Local), si.Signatures[0].SigningTime, "Signature 1: SigningTime");
            Assert.AreEqual("Zaakvoerder", si.Signatures[0].SignerRole, "Signature 1: SignerRole");
            Assert.AreEqual("Denderleeuw", si.Signatures[0].SignatureProductionPlace, "Signature 1: SignatureProductionPlace");

            //Validate timestamp validity
            Assert.AreEqual(new DateTime(2019, 1, 23, 11, 0, 0, DateTimeKind.Utc), si.TimeStampValidity, "TimeStampValidity");
        }

        [TestMethod]
        public void AnomClientVerifyDoubleSignSync()
        {
            DsspClient dsspClient = new DsspClient("https://www.e-contract.be/dss-ws/dss");

            Document d = new Document("application/pdf", File.OpenRead("SignedDouble.pdf"));
            SecurityInfo si = dsspClient.Verify(d);

            Assert.AreEqual(2, si.Signatures.Count, "Signature Count");

            //Validate signature 1
            Assert.AreEqual("SERIALNUMBER=83121034221, GIVENNAME=Iryna, SURNAME=Brouckaert, CN=Iryna Brouckaert (Signature), C=BE", si.Signatures[1].SignerSubject, "Signature 2: SignerSubject (DSS notation)");
            Assert.AreEqual("SERIALNUMBER=83121034221, G=Iryna, SN=Brouckaert, CN=Iryna Brouckaert (Signature), C=BE", si.Signatures[1].Signer.Subject, "Signature 2: Signer.Subject (Windows notation)");
            Assert.AreEqual(new DateTime(2014, 9, 23, 20, 53, 18, DateTimeKind.Local), si.Signatures[1].SigningTime, "Signature 2: SigningTime");
            Assert.AreEqual("Ik verklaar dat de inhoud van dit document nauwkeurig en volledig is", si.Signatures[1].SignerRole, "Signature 2: SignerRole");
            Assert.AreEqual("Denderleeuw", si.Signatures[1].SignatureProductionPlace, "Signature 1: SignatureProductionPlace");

            //Validate signature 2

            Assert.AreEqual("SERIALNUMBER=79021802145, GIVENNAME=Bryan Eduard, SURNAME=Brouckaert, CN=Bryan Brouckaert (Signature), C=BE", si.Signatures[0].SignerSubject, "Signature 1: SignerSubject (DSS notation)");
            Assert.AreEqual("SERIALNUMBER=79021802145, G=Bryan Eduard, SN=Brouckaert, CN=Bryan Brouckaert (Signature), C=BE", si.Signatures[0].Signer.Subject, "Signature 1: Signer.Subject (Windows notation)");
            Assert.AreEqual(new DateTime(2014, 9, 23, 20, 11, 34, DateTimeKind.Local), si.Signatures[0].SigningTime, "Signature 1: SigningTime");
            Assert.AreEqual("Zaakvoerder", si.Signatures[0].SignerRole, "Signature 1: SignerRole");
            Assert.AreEqual("Denderleeuw", si.Signatures[0].SignatureProductionPlace, "Signature 1: SignatureProductionPlace");

            //Validate timestamp validity
            Assert.AreEqual(new DateTime(2019, 1, 23, 11, 0, 0, DateTimeKind.Utc), si.TimeStampValidity, "TimeStampValidity");
        }
    }
}
