﻿/*
 *  This file is part of DSS-P client.
 *  Copyright (C) 2014 Egelke BVBA
 *  Copyright (C) 2014 e-Contract.be BVBA
 *
 *  DSS-P client is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU Lesser General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  DSS-P client is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Lesser General Public License for more details.
 *
 *  You should have received a copy of the GNU Lesser General Public License
 *  along with DSS-P client.  If not, see <http://www.gnu.org/licenses/>.
 */

using NUnit.Framework;
using System;
using System.IO;
using System.ServiceModel;

namespace EContract.Dssp.Client
{
    [TestFixture]
    public class VerifyTest
    {
        [OneTimeSetUp]
        public static void SetupClass()
        {
            String pdir = TestContext.CurrentContext.WorkDirectory;
            String wdir = Directory.GetCurrentDirectory();
            File.Copy(Path.Combine(pdir, "certificate.p12"), Path.Combine(wdir, "certificate.p12"), true);
            File.Copy(Path.Combine(pdir, "Blank.pdf"), Path.Combine(wdir, "Blank.pdf"), true);
            File.Copy(Path.Combine(pdir, "Signed.pdf"), Path.Combine(wdir, "Signed.pdf"), true);
            File.Copy(Path.Combine(pdir, "Signed.xml"), Path.Combine(wdir, "Signed.xml"), true);
            File.Copy(Path.Combine(pdir, "SignedDouble.pdf"), Path.Combine(wdir, "SignedDouble.pdf"), true);
            File.Copy(Path.Combine(pdir, "SignedStamped.pdf"), Path.Combine(wdir, "SignedStamped.pdf"), true);
        }

        [Test]
        public void VerifyPdfNoSignSync()
        {
            //Use alternative ways to specify URL
            DsspClient dsspClient = new DsspClient(new EndpointAddress("https://www.e-contract.be/dss-ws/dss"));

            Document d = new Document("application/pdf", File.OpenRead("Blank.pdf"));
            SecurityInfo si = dsspClient.Verify(d);

            //Validate lack of signature
            Assert.IsNull(si, "Verify must return null");
        }

        [Test]
        public void VerifyPdfSingleSignSync()
        {
            //Specify signature type, which isn't used here
            DsspClient dsspClient = new DsspClient(new EndpointAddress("https://www.e-contract.be/dss-ws/dss"), "urn:be:e-contract:dssp:signature:pades-baseline");

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

        //[Test]
        public void VerifyPdfDoubleSignSync()
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

        //[Test]
        public void VerifyPdfSignStampSync()
        {
            DsspClient dsspClient = new DsspClient("https://www.e-contract.be/dss-ws/dss");

            Document d = new Document("application/pdf", File.OpenRead("SignedStamped.pdf"));
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

#if NET45_OR_GREATER
        [Test]
        public void VerifyPdfSingleSignASync()
        {
            DsspClient dsspClient = new DsspClient("https://www.e-contract.be/dss-ws/dss");

            Document d = new Document("application/pdf", File.OpenRead("Signed.pdf"));
            SecurityInfo si = dsspClient.VerifyAsync(d).ConfigureAwait(false).GetAwaiter().GetResult();

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
#endif

        [Test]
        public void VerifyXmlSingleSignSync()
        {
            DsspClient dsspClient = new DsspClient("https://www.e-contract.be/dss-ws/dss");

            Document d = new Document("text/xml", File.OpenRead("Signed.xml"));
            SecurityInfo si = dsspClient.Verify(d);

            Assert.AreEqual(1, si.Signatures.Count, "Signature Count");

            //Validate signature 1
            Assert.AreEqual("SERIALNUMBER=79021802145, GIVENNAME=Bryan Eduard, SURNAME=Brouckaert, CN=Bryan Brouckaert (Signature), C=BE", si.Signatures[0].SignerSubject, "Signature 1: SignerSubject (DSS notation)");
            Assert.AreEqual("SERIALNUMBER=79021802145, G=Bryan Eduard, SN=Brouckaert, CN=Bryan Brouckaert (Signature), C=BE", si.Signatures[0].Signer.Subject, "Signature 1: Signer.Subject (Windows notation)");
            Assert.AreEqual(new DateTime(2014, 9, 23, 21, 26, 01, DateTimeKind.Local), si.Signatures[0].SigningTime, "Signature 1: SigningTime");
            Assert.IsNull(si.Signatures[0].SignerRole, "Signature 1: SignerRole");
            Assert.IsNull(si.Signatures[0].SignatureProductionPlace, "Signature 1: SignatureProductionPlace");

            //Validate timestamp validity
            Assert.AreEqual(new DateTime(2019, 1, 23, 11, 0, 0, DateTimeKind.Utc), si.TimeStampValidity, "TimeStampValidity");
        }

        
    }
}
