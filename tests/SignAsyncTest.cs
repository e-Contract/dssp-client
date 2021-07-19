using EContract.Dssp.Client.Proxy;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace EContract.Dssp.Client
{

    [TestFixture]
    public class SignAsyncTest
    {
        private static readonly TraceSource trace = new TraceSource("EContract.Dssp.Client");

        private static readonly TraceSource msgTrace = new TraceSource("EContract.Dssp.Client.MessageLogging");

        private static X509Certificate2 Signer;
        private static IdentificationResponse IdRsp = new IdentificationResponse();
        private static SignatureResponse SigRsp = new SignatureResponse();
        private static XmlSerializer XmlIdRspSer = new XmlSerializer(typeof(IdentificationResponse));
        private static XmlSerializer XmlSigRspSer = new XmlSerializer(typeof(SignatureResponse));

        [OneTimeSetUp]
        public static void ClassInit()
        {
            X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);

            X509Certificate2Collection collection = store.Certificates.Find(X509FindType.FindBySubjectName, "Bryan Brouckaert (Signature)", true);
            Signer = collection.Cast<X509Certificate2>().AsQueryable().First();

            /*
            using (Readers readers = new Readers(ReaderScope.User))
            {
                EidCard eid = (EidCard) readers.ListCards(EidCard.KNOWN_NAMES).AsQueryable().First();

                eid.Open();
                using (eid)
                {
                    IdRsp.CertificateChain.Add(eid.ReadRaw(EidFile.RrnCert));
                    IdRsp.CertificateChain.Add(eid.ReadRaw(EidFile.RootCert));

                    IdRsp.IdentityFile = eid.ReadRaw(EidFile.Id);
                    IdRsp.IdentitySignatureFile = eid.ReadRaw(EidFile.IdSig);

                    IdRsp.Photo = eid.ReadRaw(EidFile.Picture);

                    IdRsp.SigningCertificateChain.Add(eid.ReadRaw(EidFile.SignCert));
                    IdRsp.SigningCertificateChain.Add(eid.ReadRaw(EidFile.CaCert));
                    IdRsp.SigningCertificateChain.Add(eid.ReadRaw(EidFile.RootCert));

                    SigRsp.CertificateChain = IdRsp.SigningCertificateChain;
                }
            }
            */
        }

        [Test]
        public void SignAsyncNoLangInvisibleNoProps()
        {
            DsspClient dsspClient = new DsspClient("https://www.e-contract.be/dss-ws/dss");
            dsspClient.Application.UT.Name = "egelke";
            dsspClient.Application.UT.Password = "egelke";
            //dsspClient.Application.X509.Certificate = new X509Certificate2("certificate.p12", "");

            DsspSession s;
            using (Stream i = File.OpenRead("Blank.pdf"))
            {
                Document id = new Document("application/pdf", i);
                s = dsspClient.UploadDocument(id);
            }

            String signResponse = emulateBrowser(
                s.GeneratePendingRequest("http://localhost/dssp"),
                "View Document");

            NameIdentifierType signer = s.ValidateSignResponse(signResponse);
            Assert.AreEqual("SERIALNUMBER=79021802145, GIVENNAME=Bryan Eduard, SURNAME=Brouckaert, CN=Bryan Brouckaert (Signature), C=BE", signer.Value);

            Document od = dsspClient.DownloadDocument(s);            
            using (Stream o = File.OpenWrite("Output.pdf")) {
                od.Content.CopyTo(o);
            }
            od.Content.Seek(0, SeekOrigin.Current);

            Verify(od, null, null);
        }
        
        [Test]
        public void SignAsyncAuthZViaSubject()
        {
            DsspClient dsspClient = new DsspClient("https://www.e-contract.be/dss-ws/dss");
            dsspClient.Application.X509.Certificate = new X509Certificate2("certificate.p12", "");

            DsspSession s;
            using (Stream i = File.OpenRead("Blank.pdf"))
            {
                Document id = new Document("application/pdf", i);
                s = dsspClient.UploadDocument(id);
            }

            String correct = "SERIALNUMBER=79021802145, GIVENNAME=Bryan Eduard, SURNAME=Brouckaert, CN=Bryan Brouckaert (Signature), C=BE";
            Authorization authz = Authorization.AllowDssSignIfMatchSubject(correct);
            String signResponse = emulateBrowser(
                s.GeneratePendingRequest(new Uri("http://localhost/dssp"), "EN", authz),
                "View Document");
            try
            {
                using (Stream i = File.OpenRead("Blank.pdf"))
                {
                    Document id = new Document("application/pdf", i);
                    s = dsspClient.UploadDocument(id);
                }
                authz = Authorization.DenyDssSignIfMatchSubject(correct);
                signResponse = emulateBrowser(
                    s.GeneratePendingRequest(new Uri("http://localhost/dssp"), "EN", authz),
                    "View Document");
                Assert.Fail("should fail with wrong");
            }
            catch (InvalidOperationException)
            {

            }

            try
            {
                using (Stream i = File.OpenRead("Blank.pdf"))
                {
                    Document id = new Document("application/pdf", i);
                    s = dsspClient.UploadDocument(id);
                }
                authz = Authorization.AllowDssSignIfMatchSubject("SERIALNUMBER=00000000000, GIVENNAME=Bryan Eduard, SURNAME=Brouckaert, CN=Bryan Brouckaert (Signature), C=BE");
                signResponse = emulateBrowser(
                    s.GeneratePendingRequest(new Uri("http://localhost/dssp"), "EN", authz),
                    "View Document");
                Assert.Fail("should fail with wrong");
            } catch (InvalidOperationException) {

            }
        }

        [Test]
        public void SignAsyncAuthZViaSubjectRegEx()
        {
            DsspClient dsspClient = new DsspClient("https://www.e-contract.be/dss-ws/dss");
            dsspClient.Application.X509.Certificate = new X509Certificate2("certificate.p12", "");

            DsspSession s;
            using (Stream i = File.OpenRead("Blank.pdf"))
            {
                Document id = new Document("application/pdf", i);
                s = dsspClient.UploadDocument(id);
            }

            String correct = ".*Bryan Brouckaert.*";
            Authorization authz = Authorization.AllowDssSignIfMatchSubjectRegex(correct);
            String signResponse = emulateBrowser(
                s.GeneratePendingRequest(new Uri("http://localhost/dssp"), "EN", authz),
                "View Document");
            try
            {
                using (Stream i = File.OpenRead("Blank.pdf"))
                {
                    Document id = new Document("application/pdf", i);
                    s = dsspClient.UploadDocument(id);
                }
                authz = Authorization.DenyDssSignIfMatchSubjectRegex(correct);
                signResponse = emulateBrowser(
                    s.GeneratePendingRequest(new Uri("http://localhost/dssp"), "EN", authz),
                    "View Document");
                Assert.Fail("should fail with wrong");
            }
            catch (InvalidOperationException)
            {

            }

            try
            {
                using (Stream i = File.OpenRead("Blank.pdf"))
                {
                    Document id = new Document("application/pdf", i);
                    s = dsspClient.UploadDocument(id);
                }
                authz = Authorization.AllowDssSignIfMatchSubject(".*Brian Broeckaert.*");
                signResponse = emulateBrowser(
                    s.GeneratePendingRequest(new Uri("http://localhost/dssp"), "EN", authz),
                    "View Document");
                Assert.Fail("should fail with wrong");
            }
            catch (InvalidOperationException)
            {

            }
        }

        [Test]
        public void SignAsyncAuthZViaCardNr()
        {
            DsspClient dsspClient = new DsspClient("https://www.e-contract.be/dss-ws/dss");
            dsspClient.Application.X509.Certificate = new X509Certificate2("certificate.p12", "");

            DsspSession s;
            using (Stream i = File.OpenRead("Blank.pdf"))
            {
                Document id = new Document("application/pdf", i);
                s = dsspClient.UploadDocument(id);
            }

            String correct = "591591588049";
            Authorization authz = Authorization.AllowDssSignIfMatchCardNumber(correct);
            String signResponse = emulateBrowser(
                s.GeneratePendingRequest(new Uri("http://localhost/dssp"), "EN", authz),
                "View Document");
            try
            {
                using (Stream i = File.OpenRead("Blank.pdf"))
                {
                    Document id = new Document("application/pdf", i);
                    s = dsspClient.UploadDocument(id);
                }
                authz = Authorization.DenyDssSignIfMatchCardNumber(correct);
                signResponse = emulateBrowser(
                    s.GeneratePendingRequest(new Uri("http://localhost/dssp"), "EN", authz),
                    "View Document");
                Assert.Fail("should fail with wrong");
            }
            catch (InvalidOperationException)
            {

            }

            try
            {
                using (Stream i = File.OpenRead("Blank.pdf"))
                {
                    Document id = new Document("application/pdf", i);
                    s = dsspClient.UploadDocument(id);
                }
                authz = Authorization.AllowDssSignIfMatchCardNumber("591-3291070-59");
                signResponse = emulateBrowser(
                    s.GeneratePendingRequest(new Uri("http://localhost/dssp"), "EN", authz),
                    "View Document");
                Assert.Fail("should fail with wrong");
            }
            catch (InvalidOperationException)
            {

            }
        }


        [Test]
        public void SignAsyncNLInvisibleNoProps()
        {
            DsspClient dsspClient = new DsspClient("https://www.e-contract.be/dss-ws/dss");
            dsspClient.Application.X509.Certificate = new X509Certificate2("certificate.p12", "");

            DsspSession s;
            using (Stream i = File.OpenRead("Blank.pdf"))
            {
                Document id = new Document("application/pdf", i);
                s = dsspClient.UploadDocument(id);
            }

            String signResponse = emulateBrowser(
                s.GeneratePendingRequest("http://localhost/dssp", "NL"),
                "Document bekijken");

            NameIdentifierType signer = s.ValidateSignResponse(signResponse);
            Assert.AreEqual("SERIALNUMBER=79021802145, GIVENNAME=Bryan Eduard, SURNAME=Brouckaert, CN=Bryan Brouckaert (Signature), C=BE", signer.Value);

            Document od = dsspClient.DownloadDocument(s);
            using (Stream o = File.OpenWrite("Output.pdf"))
            {
                od.Content.CopyTo(o);
            }
            od.Content.Seek(0, SeekOrigin.Current);

            Verify(od, null, null);
        }


        [Test]
        public void SignAsyncNLInvisibleProps()
        {
            DsspClient dsspClient = new DsspClient("https://www.e-contract.be/dss-ws/dss");
            dsspClient.Application.X509.Certificate = new X509Certificate2("certificate.p12", "");

            DsspSession s;
            using (Stream i = File.OpenRead("Blank.pdf"))
            {
                Document id = new Document("application/pdf", i);
                s = dsspClient.UploadDocument(id);
            }

            SignatureRequestProperties props = new SignatureRequestProperties()
            {
                SignerRole = "Developer",
                SignatureProductionPlace = "Oost-Vlaanderen"
            };
            String signResponse = emulateBrowser(
                s.GeneratePendingRequest(new Uri("http://localhost/dssp"), "NL", props),
                "Document bekijken");

            NameIdentifierType signer = s.ValidateSignResponse(signResponse);
            Assert.AreEqual("SERIALNUMBER=79021802145, GIVENNAME=Bryan Eduard, SURNAME=Brouckaert, CN=Bryan Brouckaert (Signature), C=BE", signer.Value);

            Document od = dsspClient.DownloadDocument(s);
            using (Stream o = File.OpenWrite("Output.pdf"))
            {
                od.Content.CopyTo(o);
            }
            od.Content.Seek(0, SeekOrigin.Current);

            Verify(od, "Developer", "Oost-Vlaanderen");
        }


        [Test]
        public void SignAsyncNLVisibleProps()
        {
            DsspClient dsspClient = new DsspClient("https://www.e-contract.be/dss-ws/dss");
            dsspClient.Application.UT.Name = "egelke";
            dsspClient.Application.UT.Password = "egelke";

            DsspSession s;
            using (Stream i = File.OpenRead("Blank.pdf"))
            {
                Document id = new Document("application/pdf", i);
                s = dsspClient.UploadDocument(id);
            }

            SignatureRequestProperties props = new SignatureRequestProperties()
            {
                SignerRole = "Developer",
                SignatureProductionPlace = "Oost-Vlaanderen",
                VisibleSignature = new ImageVisibleSignature()
                {
                    CustomText = "My Text",
                    ValueUri = "urn:be:e-contract:dssp:1.0:vs:si:eid-photo:signer-info",
                    Page = 1,
                    X = 500,
                    Y = 700
                }
            };
            String signResponse = emulateBrowser(
                s.GeneratePendingRequest(new Uri("http://localhost/dssp"), "NL", props),
                "Document bekijken");

            NameIdentifierType signer = s.ValidateSignResponse(signResponse);
            Assert.AreEqual("SERIALNUMBER=79021802145, GIVENNAME=Bryan Eduard, SURNAME=Brouckaert, CN=Bryan Brouckaert (Signature), C=BE", signer.Value);

            Document od = dsspClient.DownloadDocument(s);
            using (Stream o = File.OpenWrite("Output.pdf"))
            {
                od.Content.CopyTo(o);
            }
            od.Content.Seek(0, SeekOrigin.Current);

            Verify(od, "Developer", "Oost-Vlaanderen");
        }

        [Test]
        public void SignAsyncNLVisiblePropsMultiText()
        {
            DsspClient dsspClient = new DsspClient("https://www.e-contract.be/dss-ws/dss");
            dsspClient.Application.UT.Name = "egelke";
            dsspClient.Application.UT.Password = "egelke";

            DsspSession s;
            using (Stream i = File.OpenRead("Blank.pdf"))
            {
                Document id = new Document("application/pdf", i);
                s = dsspClient.UploadDocument(id);
            }

            SignatureRequestProperties props = new SignatureRequestProperties()
            {
                SignerRole = "Developer",
                SignatureProductionPlace = "Oost-Vlaanderen",
                VisibleSignature = new ImageVisibleSignature()
                {
                    CustomText = "Custom",
                    CustomText2 = "Custom2",
                    CustomText3 = "Custom3",
                    CustomText4 = "Custom4",
                    CustomText5 = "Custom5",
                    ValueUri = "urn:be:e-contract:dssp:1.0:vs:si:eid-photo:signer-info",
                    Page = 1,
                    X = 500,
                    Y = 700
                }
            };
            String signResponse = emulateBrowser(
                s.GeneratePendingRequest(new Uri("http://localhost/dssp"), "NL", props),
                "Document bekijken");

            NameIdentifierType signer = s.ValidateSignResponse(signResponse);
            Assert.AreEqual("SERIALNUMBER=79021802145, GIVENNAME=Bryan Eduard, SURNAME=Brouckaert, CN=Bryan Brouckaert (Signature), C=BE", signer.Value);

            Document od = dsspClient.DownloadDocument(s);
            using (Stream o = File.OpenWrite("Output.pdf"))
            {
                od.Content.CopyTo(o);
            }
            od.Content.Seek(0, SeekOrigin.Current);

            Verify(od, "Developer", "Oost-Vlaanderen");
        }

        [Test]
        public void SignAsyncWithSerialization()
        {
            MemoryStream ss = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();
            DsspClient dsspClient = new DsspClient("https://www.e-contract.be/dss-ws/dss");
            dsspClient.Application.X509.Certificate = new X509Certificate2("certificate.p12", "");

            DsspSession s;
            using (Stream i = File.OpenRead("Blank.pdf"))
            {
                Document id = new Document("application/pdf", i);
                s = dsspClient.UploadDocument(id);
            }

            formatter.Serialize(ss, s);
            ss.Seek(0, SeekOrigin.Begin);
            s = (DsspSession)formatter.Deserialize(ss);

            String pr = s.GeneratePendingRequest("http://localhost/dssp");

            formatter.Serialize(ss, s);

            String signResponse = emulateBrowser(pr, "View Document");

            ss.Seek(0, SeekOrigin.Begin);
            s = (DsspSession)formatter.Deserialize(ss);

            NameIdentifierType signer = s.ValidateSignResponse(signResponse);
            Assert.AreEqual("SERIALNUMBER=79021802145, GIVENNAME=Bryan Eduard, SURNAME=Brouckaert, CN=Bryan Brouckaert (Signature), C=BE", signer.Value);

            formatter.Serialize(ss, s);
            ss.Seek(0, SeekOrigin.Begin);
            s = (DsspSession)formatter.Deserialize(ss);

            Document od = dsspClient.DownloadDocument(s);
            using (Stream o = File.OpenWrite("Output.pdf"))
            {
                od.Content.CopyTo(o);
            }
            od.Content.Seek(0, SeekOrigin.Current);

            Verify(od, null, null);
        }

        private String emulateBrowser(String pendingRequest, String title)
        {
            HttpWebRequest request;
            HttpWebResponse rsp;
            CookieContainer cookies = new CookieContainer();

            //Post pending request
            request = (HttpWebRequest) WebRequest.Create("https://www.e-contract.be/dss-ws/start");
            request.AllowAutoRedirect = false;
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.CookieContainer = cookies;
            using (TextWriter requestBody = new StreamWriter(request.GetRequestStream()))
            {
                requestBody.Write("PendingRequest=");
                requestBody.Write(HttpUtility.UrlEncode(pendingRequest));
            }
            rsp = (HttpWebResponse) request.GetResponse();
            Assert.AreEqual(HttpStatusCode.Redirect, rsp.StatusCode, "pending request response status");
            Assert.AreEqual("https://www.e-contract.be/dss-ws/view.xhtml?faces-redirect=true", rsp.Headers[HttpResponseHeader.Location], "pending request response redirect location");

            //obtain view
            request = (HttpWebRequest)WebRequest.Create(rsp.Headers[HttpResponseHeader.Location]);
            request.CookieContainer = cookies;
            rsp = (HttpWebResponse) request.GetResponse();
            using(TextReader responseBody = new StreamReader(rsp.GetResponseStream())) {
                String view = responseBody.ReadToEnd();
                Assert.IsTrue(view.Contains(title), "View page is in the correct language");
            }

            //Applet Init: send version ignore response
            //var xmlVrsRsp = new XmlDocument() { PreserveWhitespace = true };
            String versionRsp = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><eid:VersionResponse xmlns:eid=\"urn:be:e-contract:eid:protocol:1.0.0\" xmlns:eid110=\"urn:be:e-contract:eid:protocol:1.1.0\" xmlns:ds=\"http://www.w3.org/2000/09/xmldsig#\"><eid:ExtensionVersion>1.0.0</eid:ExtensionVersion><eid:UserAgent>Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36</eid:UserAgent><eid:MiddlewareVersion>1.3.1</eid:MiddlewareVersion><eid:JavaVersion>1.8.0_291</eid:JavaVersion><eid:JavaVendor>Oracle Corporation</eid:JavaVendor><eid:OperatingSystem>Windows 10</eid:OperatingSystem><eid:OperatingSystemVersion>10.0</eid:OperatingSystemVersion><eid:OperatingSystemArchitecture>amd64</eid:OperatingSystemArchitecture></eid:VersionResponse>";
            request = (HttpWebRequest)WebRequest.Create("https://www.e-contract.be/dss-ws/chrome-service");
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36";
            request.Method = "POST";
            request.ContentType = "text/xml";
            request.CookieContainer = cookies;
            using (TextWriter requestBody = new StreamWriter(request.GetRequestStream()))
            {
                requestBody.Write(versionRsp);
            }
            rsp = (HttpWebResponse)request.GetResponse();
            Assert.AreEqual(HttpStatusCode.OK, rsp.StatusCode, "eid:VersionResponse status");
            using (TextReader responseBody = new StreamReader(rsp.GetResponseStream()))
            {
                String view = responseBody.ReadToEnd();
                Assert.IsTrue(view.Contains("IdentificationRequest"), "The version response is invalid");
            }

            return String.Empty;
        }

        /*
        private String emulateBrowser(String pendingRequest, String title)
        {
            WebRequestHandler webRequestHandler = new WebRequestHandler();
            webRequestHandler.AllowAutoRedirect = false;
            HttpClient client = new HttpClient(webRequestHandler);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/56.0.2924.87 Safari/537.36");

            //Post pending request
            KeyValuePair<string, string> pendingRequestFormEntry = new KeyValuePair<string, string>("PendingRequest", pendingRequest);
            FormUrlEncodedContent pendingRequestForm = new FormUrlEncodedContent(new KeyValuePair<string, string>[] { pendingRequestFormEntry });
            trace.TraceEvent(TraceEventType.Information, 0, "browser: start");
            HttpResponseMessage rsp = client.PostAsync("https://www.e-contract.be/dss-ws/start", pendingRequestForm).Result;
            Assert.AreEqual(System.Net.HttpStatusCode.Redirect, rsp.StatusCode, "pending request response status");
            Assert.AreEqual(new Uri("https://www.e-contract.be/dss-ws/view.xhtml?faces-redirect=true"), rsp.Headers.Location, "pending request response redirect location");

            //obtain view
            String view = client.GetStringAsync(rsp.Headers.Location).Result;
            Assert.IsTrue(view.Contains(title), "View page is in the correct language");

            //Applet Init: send version ignore response
            var xmlVrsRsp = new XmlDocument() { PreserveWhitespace = true };
            xmlVrsRsp.LoadXml("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><eid:VersionResponse xmlns:eid=\"urn:be:e-contract:eid:protocol:1.0.0\" xmlns:eid110=\"urn:be:e-contract:eid:protocol:1.1.0\" xmlns:ds=\"http://www.w3.org/2000/09/xmldsig#\"><eid:ExtensionVersion>1.0.0</eid:ExtensionVersion><eid:UserAgent>Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/56.0.2924.87 Safari/537.36</eid:UserAgent><eid:MiddlewareVersion>1.1.5</eid:MiddlewareVersion><eid:JavaVersion>1.8.0_121</eid:JavaVersion><eid:JavaVendor>Oracle Corporation</eid:JavaVendor><eid:OperatingSystem>Windows 10</eid:OperatingSystem><eid:OperatingSystemVersion>10.0</eid:OperatingSystemVersion><eid:OperatingSystemArchitecture>amd64</eid:OperatingSystemArchitecture></eid:VersionResponse>");
            trace.TraceEvent(TraceEventType.Information, 0, "browser: send versions");
            msgTrace.TraceData(TraceEventType.Information, 0, xmlVrsRsp.CreateNavigator());
            var stream = new MemoryStream();
            xmlVrsRsp.Save(stream);
            rsp = client.PostAsync("https://www.e-contract.be/dss-ws/chrome-service", new ByteArrayContent(stream.ToArray())).Result;
            Assert.AreEqual(System.Net.HttpStatusCode.OK, rsp.StatusCode, "eid:VersionResponse status");

            //Applet Identify: Send Belpic file info, get hash from response
            var xmlIdRsp = new XmlDocument() { PreserveWhitespace = true };
            var xmlIdRspNav = xmlIdRsp.CreateNavigator();
            using (var writer = xmlIdRspNav.AppendChild())
            {
                XmlIdRspSer.Serialize(writer, IdRsp);
            }
            trace.TraceEvent(TraceEventType.Information, 0, "browser: send ID information");
            msgTrace.TraceData(TraceEventType.Information, 0, xmlIdRspNav);
            stream = new MemoryStream();
            xmlIdRsp.Save(stream);
            rsp = client.PostAsync("https://www.e-contract.be/dss-ws/chrome-service", new ByteArrayContent(stream.ToArray())).Result;
            Assert.AreEqual(System.Net.HttpStatusCode.OK, rsp.StatusCode, "eid:IdentificationResponse status");
            Stream signatureRequestStream = rsp.Content.ReadAsStreamAsync().Result;
            XmlDocument doc = new XmlDocument();
            doc.Load(signatureRequestStream);
            if (doc.DocumentElement.LocalName == "FinishRequest")
                throw new InvalidOperationException();
            byte[] digestValue = Convert.FromBase64String(doc.GetElementsByTagName("DigestValue", "urn:be:e-contract:eid:protocol:1.0.0")[0].InnerText);
            String digestAlgo = doc.GetElementsByTagName("DigestAlgorithm", "urn:be:e-contract:eid:protocol:1.0.0")[0].InnerText;

            //Do actual signing
            var key = (RSACryptoServiceProvider)Signer.PrivateKey;
            String digestOid = CryptoConfig.MapNameToOID(CryptoConfig.CreateFromName(digestAlgo).GetType().ToString());
            SigRsp.SignatureValue = key.SignHash(digestValue, digestOid);

            //Applet Sign: send proof
            var xmlSigRsp = new XmlDocument() { PreserveWhitespace = true };
            var xmlSigRspNav = xmlSigRsp.CreateNavigator();
            using (var writer = xmlSigRspNav.AppendChild())
            {
                XmlSigRspSer.Serialize(writer, SigRsp);
            }
            trace.TraceEvent(TraceEventType.Information, 0, "browser: send signature value");
            msgTrace.TraceData(TraceEventType.Information, 0, xmlSigRspNav);
            stream = new MemoryStream();
            xmlSigRsp.Save(stream);
            rsp = client.PostAsync("https://www.e-contract.be/dss-ws/chrome-service", new ByteArrayContent(stream.ToArray())).Result;
            Assert.AreEqual(System.Net.HttpStatusCode.OK, rsp.StatusCode, "eid:SignatureResponse status");

            //Applet End
            trace.TraceEvent(TraceEventType.Information, 0, "browser: get the signed result (expecte redirect)");
            rsp = client.GetAsync("https://www.e-contract.be/dss-ws/signed-chrome?message=Native%20host%20has%20exited").Result;
            Assert.AreEqual(System.Net.HttpStatusCode.Redirect, rsp.StatusCode, "pending request response status");
            Assert.AreEqual(new Uri("https://www.e-contract.be/dss-ws/signed.xhtml"), rsp.Headers.Location, "pending request response redirect location");

            //obtain response
            trace.TraceEvent(TraceEventType.Information, 0, "browser: get the actual signed result");
            String signed = client.GetStringAsync("https://www.e-contract.be/dss-ws/signed.xhtml").Result;
            String searchkey = "<input type=\"hidden\" name=\"SignResponse\" value=\"";
            int startIndex = signed.IndexOf(searchkey) + searchkey.Length;
            int endIndex = signed.IndexOf('"', startIndex);
            return signed.Substring(startIndex, endIndex - startIndex);
        }
        */

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
