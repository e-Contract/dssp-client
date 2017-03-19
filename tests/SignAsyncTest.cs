using EContract.Dssp.Client.Proxy;
using Microsoft.Owin.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Owin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Xml;
using System.Xml.Linq;

namespace EContract.Dssp.Client
{

    [TestClass]
    public class SignAsyncTest
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
        public void SignAsyncNoLangInvisibleNoProps()
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

        //[TestMethod]
        public void SignAsyncAuthZViaSubjectPossitive()
        {
            DsspClient dsspClient = new DsspClient("https://www.e-contract.be/dss-ws/dss");
            dsspClient.Application.X509.Certificate = new X509Certificate2("certificate.p12", "");

            DsspSession s;
            using (Stream i = File.OpenRead("Blank.pdf"))
            {
                Document id = new Document("application/pdf", i);
                s = dsspClient.UploadDocument(id);
            }

            Authorization authz = Authorization.AllowDssSignIfMatchSubject("SERIALNUMBER = 79021802145, GIVENNAME = Bryan Eduard, SURNAME = Brouckaert, CN = Bryan Brouckaert(Signature), C = BE");
            String signResponse = emulateBrowser(
                s.GeneratePendingRequest(new Uri("http://localhost/dssp"), "EN", authz),
                "View Document");

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

        [TestMethod]
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


        [TestMethod]
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


        [TestMethod]
        public void SignAsyncNLVisibleProps()
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

        private String emulateBrowser(String pendingRequest, String title)
        {
            WebRequestHandler webRequestHandler = new WebRequestHandler();
            webRequestHandler.AllowAutoRedirect = false;
            HttpClient client = new HttpClient(webRequestHandler);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/56.0.2924.87 Safari/537.36");

            //Post pending request
            KeyValuePair<string, string> pendingRequestFormEntry = new KeyValuePair<string, string>("PendingRequest", pendingRequest);
            FormUrlEncodedContent pendingRequestForm = new FormUrlEncodedContent(new KeyValuePair<string, string>[] { pendingRequestFormEntry });
            HttpResponseMessage rsp = client.PostAsync("https://www.e-contract.be/dss-ws/start", pendingRequestForm).Result;
            Assert.AreEqual(System.Net.HttpStatusCode.Redirect, rsp.StatusCode, "pending request response status");
            Assert.AreEqual(new Uri("https://www.e-contract.be/dss-ws/view.xhtml"), rsp.Headers.Location, "pending request response redirect location");

            //obtain view
            String view = client.GetStringAsync(rsp.Headers.Location).Result;
            Assert.IsTrue(view.Contains(title), "View page is in the correct language");

            //Applet Init: send version ignore response
            rsp = client.PostAsync("https://www.e-contract.be/dss-ws/chrome-service", new StringContent("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><eid:VersionResponse xmlns:eid=\"urn:be:e-contract:eid:protocol:1.0.0\" xmlns:eid110=\"urn:be:e-contract:eid:protocol:1.1.0\" xmlns:ds=\"http://www.w3.org/2000/09/xmldsig#\"><eid:ExtensionVersion>1.0.0</eid:ExtensionVersion><eid:UserAgent>Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/56.0.2924.87 Safari/537.36</eid:UserAgent><eid:MiddlewareVersion>1.1.5</eid:MiddlewareVersion><eid:JavaVersion>1.8.0_121</eid:JavaVersion><eid:JavaVendor>Oracle Corporation</eid:JavaVendor><eid:OperatingSystem>Windows 10</eid:OperatingSystem><eid:OperatingSystemVersion>10.0</eid:OperatingSystemVersion><eid:OperatingSystemArchitecture>amd64</eid:OperatingSystemArchitecture></eid:VersionResponse>", Encoding.UTF8)).Result;
            Assert.AreEqual(System.Net.HttpStatusCode.OK, rsp.StatusCode, "eid:VersionResponse status");

            //Applet Identify: Send Belpic file info, get hash from response
            rsp = client.PostAsync("https://www.e-contract.be/dss-ws/chrome-service", new StringContent("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><eid:IdentificationResponse xmlns:eid=\"urn:be:e-contract:eid:protocol:1.0.0\" xmlns:eid110=\"urn:be:e-contract:eid:protocol:1.1.0\" xmlns:ds=\"http://www.w3.org/2000/09/xmldsig#\"><eid:CertificateChain><eid:Certificate>MIIDLzCCAhegAwIBAgILBAAAAAABNl6mFfAwDQYJKoZIhvcNAQEFBQAwKDELMAkGA1UEBhMCQkUxGTAXBgNVBAMTEEJlbGdpdW0gUm9vdCBDQTIwHhcNMTIwMzI5MTEwMDAwWhcNMTgxMTI5MTEwMDAwWjApMQswCQYDVQQGEwJCRTEMMAoGA1UEChMDUlJOMQwwCgYDVQQDEwNSUk4wgZ8wDQYJKoZIhvcNAQEBBQADgY0AMIGJAoGBALVwEAWX2cb7qVBoCLglx5R0W8DAKgtNki2BwDPr1DsV5bkr75a3OFVN822rbxUb6747mFOr3Tqsc0xQ5SsLq7EaMhmokml2SoqIzMCgAHQTgRzPt1eDJkRcqiL/AV/9UblFB8QTaDO6l4+fAi4KZky1OVvBcI6rvYV2B2KlLW91AgMBAAGjgdwwgdkwDgYDVR0PAQH/BAQDAgbAMEMGA1UdIAQ8MDowOAYGYDgJAQEEMC4wLAYIKwYBBQUHAgEWIGh0dHA6Ly9yZXBvc2l0b3J5LmVpZC5iZWxnaXVtLmJlMB0GA1UdDgQWBBSEL5kBtZViDvy5evz1KOvgkiDmwTA3BgNVHR8EMDAuMCygKqAohiZodHRwOi8vY3JsLmVpZC5iZWxnaXVtLmJlL2JlbGdpdW0yLmNybDAJBgNVHRMEAjAAMB8GA1UdIwQYMBaAFIWK6/TFu74OWQOU3taAARXjEJw5MA0GCSqGSIb3DQEBBQUAA4IBAQBGKmv+1NwqOmvcCxSNkYQJ9iqPX3bhXPFMh/x5kX9R1VhpCSukV+z3SQl190vhHajEKNHAB/ZQIe3Q2w9SQsURS3pWQ+U11q1j2OAPHxFfm8vu+/CHrjVKTjsmvxprqX9EegQNGOrcsYBhlxj7+hWcDndmBuQRa4RFxphJdQiG2jgdnKb6qqJUCwsd4nAkJGQ/zQamaqgkVoRHcHykHG9RpdeByH/xghALbcmDmGhVCisKd8phjjeyG/Cs9NnoGbdNcPkHjz6FwCSuTzKV1M+4h8+QyRM80jUGM+e99eywTrh0FtBIvBvIlVI6Ge6sRQYzd4hNrJTSMRZYE6WeJ1oR</eid:Certificate><eid:Certificate>MIIDjjCCAnagAwIBAgIIKv++n6Lw6YcwDQYJKoZIhvcNAQEFBQAwKDELMAkGA1UEBhMCQkUxGTAXBgNVBAMTEEJlbGdpdW0gUm9vdCBDQTIwHhcNMDcxMDA0MTAwMDAwWhcNMjExMjE1MDgwMDAwWjAoMQswCQYDVQQGEwJCRTEZMBcGA1UEAxMQQmVsZ2l1bSBSb290IENBMjCCASIwDQYJKoZIhvcNAQEBBQADggEPADCCAQoCggEBAMZzQh6S/3UPi790hqc/7bIYLS2X+an7mEoj39WN4IzGMhwWLQdC1i22bi+n9fzGhYJdld61IgDMqFNAn68KNaJ6x+HK92AQZw6nUHMXU5WfIp8MXW+2QbyM69odRr2nlL/zGsvU+40OHjPIltfsjFPekx40HopQcSZYtF3CiInaYNKJIT/e1wEYNm7hLHADBGXvmAYrXR5i3FVr/mZkIV/4L+HXmymvb82fqgxG0YjFnaKVn6w/Fa7yYd/vw2uaItgscf1YHewApDgglVrH1Tdjuk+bqv5WRi5j2Qsj1Yr6tSPwiRuhFA0m2kHwOI8w7QUmecFLTqG4flVSOmlGhHUCAwEAAaOBuzCBuDAOBgNVHQ8BAf8EBAMCAQYwDwYDVR0TAQH/BAUwAwEB/zBCBgNVHSAEOzA5MDcGBWA4CQEBMC4wLAYIKwYBBQUHAgEWIGh0dHA6Ly9yZXBvc2l0b3J5LmVpZC5iZWxnaXVtLmJlMB0GA1UdDgQWBBSFiuv0xbu+DlkDlN7WgAEV4xCcOTARBglghkgBhvhCAQEEBAMCAAcwHwYDVR0jBBgwFoAUhYrr9MW7vg5ZA5Te1oABFeMQnDkwDQYJKoZIhvcNAQEFBQADggEBAFHYhd27V2/MoGy1oyCcUwnzSgEMdL8rs5qauhjyC4isHLMzr87lEwEnkoRYmhC598wUkmt0FoqW6FHvv/pKJaeJtmMrXZRY0c8RcrYeuTlBFk0pvDVTC9rejg7NqZV3JcqUWumyaa7YwBO+mPyWnIR/VRPmPIfjvCCkpDZoa01gZhz5v6yAlGYuuUGK02XThIAC71AdXkbc98m6tTR8KvPG2F9fVJ3bTc0R5/0UAoNmXsimABKgX77OFP67H6dh96tK8QYUn8pJQsKpvO2FsauBQeYNxUJpU4c5nUwfAA4+Bw11V0SoU7Q2dmSZ3G7rPUZuFF1eR1ONeE3gJ7uOhXY=</eid:Certificate></eid:CertificateChain><eid:IdentityFile>AQw1OTE1OTE1ODgwNDkCEFNMSU4zZgATkw0iDS8IIzQDCjMxLjA1LjIwMTIECjMxLjA1LjIwMTcFC0RlbmRlcmxlZXV3Bgs3OTAyMTgwMjE0NQcKQnJvdWNrYWVydAgMQnJ5YW4gRWR1YXJkCQFHCgRCZWxnCwZWZXVybmUMDDE4IEZFQiAgMTk3OQ0BTQ4ADwIwMRABMBEUKJSF8tRZNpkwjFEt+VuFsy/wpvE=</eid:IdentityFile><eid:IdentitySignatureFile>FTuN9EqicpuOGbHduxuF46pTdamDSi6dgx/+0455o8kP1SYD/aYn729VvaA/J6FIY6v3NgpJG8ddE1/P6+MSdP0pyiwW6fwxUgcaZ22pi2l5b8dwIF2/NF9zVKWDKir0Ck1E+zNJ3r72z/BanwCGh8v8qM3F1OXkyQNMFenqG6I=</eid:IdentitySignatureFile><eid:Photo>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</eid:Photo><eid:SigningCertificateChain><eid:Certificate>MIIEBDCCAuygAwIBAgIQEAAAAAAASsTUP4MA6uOdgTANBgkqhkiG9w0BAQUFADAzMQswCQYDVQQGEwJCRTETMBEGA1UEAxMKQ2l0aXplbiBDQTEPMA0GA1UEBRMGMjAxMjA0MB4XDTEyMDYwNTE2MzYxNVoXDTE3MDUzMTIzNTk1OVowdjELMAkGA1UEBhMCQkUxJTAjBgNVBAMTHEJyeWFuIEJyb3Vja2FlcnQgKFNpZ25hdHVyZSkxEzARBgNVBAQTCkJyb3Vja2FlcnQxFTATBgNVBCoTDEJyeWFuIEVkdWFyZDEUMBIGA1UEBRMLNzkwMjE4MDIxNDUwgZ8wDQYJKoZIhvcNAQEBBQADgY0AMIGJAoGBAIEW/uUSjcZqISYHVj1gJjFbgdWOpq/Z5kcnqx0CY25IscqXsZEEeODA44SehmBzLfvS3su5+7utNXE1IwNTVTtvSxPSfcaklDO1bAJOifbrQRAKMdm9zcACDvKWFdnpsHGq0f/BhYxuXFNd00w11TwiIi/fRViwV4EqWmAwJ9+PAgMBAAGjggFTMIIBTzAfBgNVHSMEGDAWgBT4Jg45QH6chvZArU9E+qmhpUOt+TBuBggrBgEFBQcBAQRiMGAwNgYIKwYBBQUHMAKGKmh0dHA6Ly9jZXJ0cy5laWQuYmVsZ2l1bS5iZS9iZWxnaXVtcnMyLmNydDAmBggrBgEFBQcwAYYaaHR0cDovL29jc3AuZWlkLmJlbGdpdW0uYmUwRAYDVR0gBD0wOzA5BgdgOAkBAQIBMC4wLAYIKwYBBQUHAgEWIGh0dHA6Ly9yZXBvc2l0b3J5LmVpZC5iZWxnaXVtLmJlMDkGA1UdHwQyMDAwLqAsoCqGKGh0dHA6Ly9jcmwuZWlkLmJlbGdpdW0uYmUvZWlkYzIwMTIwNC5jcmwwDgYDVR0PAQH/BAQDAgZAMBEGCWCGSAGG+EIBAQQEAwIFIDAYBggrBgEFBQcBAwQMMAowCAYGBACORgEBMA0GCSqGSIb3DQEBBQUAA4IBAQCD2ZNn1irgPGOs7SXkXyiFP+J2Swwl9bRFCTq44WhN1wSfSbglGd7WBVnBLhnuoasF5G8QY0Vt6LA9DYRKP2WYkvEfRADmJVBXOzQc/X4T/kGHddmHyk8uPKuKoHvu0fJ8l30WuioPjmNx4lDAlHyLw5lMmlEqGkNeKb+LaPsceULGfZsDdSb6Kzl6LABY5a4zhYz+fOoq3HnlxHbK9uJNb/yL2TgI57IFFg7/Rcpm/DDgZHknd++TPBE+W9/wQ5k/YzCk6bjW206c8tlrtkZIxwVCEQMzpDCJp8InoUqnlFxTXSLLTxdsLpU2riyxPuNC5Qes50XGy+WCOgGLygbb</eid:Certificate><eid:Certificate>MIID3jCCAsagAwIBAgIQc0D0rXT4VSFKh5sH3aRxDjANBgkqhkiG9w0BAQUFADAoMQswCQYDVQQGEwJCRTEZMBcGA1UEAxMQQmVsZ2l1bSBSb290IENBMjAeFw0xMTExMjMxMTAwMDBaFw0xODA3MjMxMTAwMDBaMDMxCzAJBgNVBAYTAkJFMRMwEQYDVQQDEwpDaXRpemVuIENBMQ8wDQYDVQQFEwYyMDEyMDQwggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQCXd7fTaijI+u7jXAjQxTw4PwHo3tuXLoZHYndqGAYVFfL3hj3cz0V7177WD3ZTxrqTDK8r8IxFogcQYwQZi6JzyT2HE+x9tgFy5OOKo/tecO/eeXX/hX6/QnmoJIQn2sMjdQo4TbOMJVyqIAziONU2Fv3J/p31ZPcGrBwB6DgMYlZ2/9CepREu3dHVcFG8NWai45rFE0M1UO3/zKoXOKTEZLTKR15jmfHMIcRvN5Yxj8VcrBCfyYaiBjGOc7wLPzJju7yaYnr+ZtzGfUpPTKGggyl6nDNJlY9EIFIFxmp4LUKlPWliGgChZM6FA1w2dunfyLEGHJ73eyqWjh2ikM6tAgMBAAGjgfgwgfUwDgYDVR0PAQH/BAQDAgEGMBIGA1UdEwEB/wQIMAYBAf8CAQAwQwYDVR0gBDwwOjA4BgZgOAkBAQIwLjAsBggrBgEFBQcCARYgaHR0cDovL3JlcG9zaXRvcnkuZWlkLmJlbGdpdW0uYmUwHQYDVR0OBBYEFPgmDjlAfpyG9kCtT0T6qaGlQ635MDcGA1UdHwQwMC4wLKAqoCiGJmh0dHA6Ly9jcmwuZWlkLmJlbGdpdW0uYmUvYmVsZ2l1bTIuY3JsMBEGCWCGSAGG+EIBAQQEAwIABzAfBgNVHSMEGDAWgBSFiuv0xbu+DlkDlN7WgAEV4xCcOTANBgkqhkiG9w0BAQUFAAOCAQEAlDk/FgLlMRzjn4Bw8CYhwi/qAyojus4cPOncKFa1PYlzHQNIabYBrll3TMp7cWEe4hnRy/rrCOsBQABZd0Qbbgnh95H5A5nRFmxGGW+OzQYmScWjrHdLIo+Ti+hPNUwoBjtreW53OEbiWHw3k3+HC+eUBLNBBu0HlqC8PWQDVxurxyHibLi+F9NzWiWa3EM7ANmNLAhU1uga3aQ2iiaXIRozIn13N8OHUEKfpS4ACW2nXb1AB4cMKJ+ACT6VD6ArUd9zGiz/D6F5BUbSH/2ZqtF6GHPx0fKET1ovdIUGZfZmOAZLZVC67IC4FlsYAklRyrCQZOTNxx1gFgn60ldzSA==</eid:Certificate><eid:Certificate>MIIDjjCCAnagAwIBAgIIKv++n6Lw6YcwDQYJKoZIhvcNAQEFBQAwKDELMAkGA1UEBhMCQkUxGTAXBgNVBAMTEEJlbGdpdW0gUm9vdCBDQTIwHhcNMDcxMDA0MTAwMDAwWhcNMjExMjE1MDgwMDAwWjAoMQswCQYDVQQGEwJCRTEZMBcGA1UEAxMQQmVsZ2l1bSBSb290IENBMjCCASIwDQYJKoZIhvcNAQEBBQADggEPADCCAQoCggEBAMZzQh6S/3UPi790hqc/7bIYLS2X+an7mEoj39WN4IzGMhwWLQdC1i22bi+n9fzGhYJdld61IgDMqFNAn68KNaJ6x+HK92AQZw6nUHMXU5WfIp8MXW+2QbyM69odRr2nlL/zGsvU+40OHjPIltfsjFPekx40HopQcSZYtF3CiInaYNKJIT/e1wEYNm7hLHADBGXvmAYrXR5i3FVr/mZkIV/4L+HXmymvb82fqgxG0YjFnaKVn6w/Fa7yYd/vw2uaItgscf1YHewApDgglVrH1Tdjuk+bqv5WRi5j2Qsj1Yr6tSPwiRuhFA0m2kHwOI8w7QUmecFLTqG4flVSOmlGhHUCAwEAAaOBuzCBuDAOBgNVHQ8BAf8EBAMCAQYwDwYDVR0TAQH/BAUwAwEB/zBCBgNVHSAEOzA5MDcGBWA4CQEBMC4wLAYIKwYBBQUHAgEWIGh0dHA6Ly9yZXBvc2l0b3J5LmVpZC5iZWxnaXVtLmJlMB0GA1UdDgQWBBSFiuv0xbu+DlkDlN7WgAEV4xCcOTARBglghkgBhvhCAQEEBAMCAAcwHwYDVR0jBBgwFoAUhYrr9MW7vg5ZA5Te1oABFeMQnDkwDQYJKoZIhvcNAQEFBQADggEBAFHYhd27V2/MoGy1oyCcUwnzSgEMdL8rs5qauhjyC4isHLMzr87lEwEnkoRYmhC598wUkmt0FoqW6FHvv/pKJaeJtmMrXZRY0c8RcrYeuTlBFk0pvDVTC9rejg7NqZV3JcqUWumyaa7YwBO+mPyWnIR/VRPmPIfjvCCkpDZoa01gZhz5v6yAlGYuuUGK02XThIAC71AdXkbc98m6tTR8KvPG2F9fVJ3bTc0R5/0UAoNmXsimABKgX77OFP67H6dh96tK8QYUn8pJQsKpvO2FsauBQeYNxUJpU4c5nUwfAA4+Bw11V0SoU7Q2dmSZ3G7rPUZuFF1eR1ONeE3gJ7uOhXY=</eid:Certificate></eid:SigningCertificateChain></eid:IdentificationResponse>", Encoding.UTF8)).Result;
            Assert.AreEqual(System.Net.HttpStatusCode.OK, rsp.StatusCode, "eid:IdentificationResponse status");
            Stream signatureRequestStream = rsp.Content.ReadAsStreamAsync().Result;
            XmlDocument doc = new XmlDocument();
            doc.Load(signatureRequestStream);
            byte[] digestValue = Convert.FromBase64String(doc.GetElementsByTagName("DigestValue", "urn:be:e-contract:eid:protocol:1.0.0")[0].InnerText);
            String digestAlgo = doc.GetElementsByTagName("DigestAlgorithm", "urn:be:e-contract:eid:protocol:1.0.0")[0].InnerText;

            //Do actual signing
            var key = (RSACryptoServiceProvider)Signer.PrivateKey;
            String digestOid = CryptoConfig.MapNameToOID(CryptoConfig.CreateFromName(digestAlgo).GetType().ToString());
            byte[] signValue = key.SignHash(digestValue, digestOid);
            String signValueBase64 = Convert.ToBase64String(signValue);

            //Applet Sign: send proof
            rsp = client.PostAsync("https://www.e-contract.be/dss-ws/chrome-service", new StringContent("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><eid:SignatureResponse xmlns:eid=\"urn:be:e-contract:eid:protocol:1.0.0\" xmlns:eid110=\"urn:be:e-contract:eid:protocol:1.1.0\" xmlns:ds=\"http://www.w3.org/2000/09/xmldsig#\"><eid:SignatureValue>" + signValueBase64 + "</eid:SignatureValue><eid:CertificateChain><eid:Certificate>MIIEBDCCAuygAwIBAgIQEAAAAAAASsTUP4MA6uOdgTANBgkqhkiG9w0BAQUFADAzMQswCQYDVQQGEwJCRTETMBEGA1UEAxMKQ2l0aXplbiBDQTEPMA0GA1UEBRMGMjAxMjA0MB4XDTEyMDYwNTE2MzYxNVoXDTE3MDUzMTIzNTk1OVowdjELMAkGA1UEBhMCQkUxJTAjBgNVBAMTHEJyeWFuIEJyb3Vja2FlcnQgKFNpZ25hdHVyZSkxEzARBgNVBAQTCkJyb3Vja2FlcnQxFTATBgNVBCoTDEJyeWFuIEVkdWFyZDEUMBIGA1UEBRMLNzkwMjE4MDIxNDUwgZ8wDQYJKoZIhvcNAQEBBQADgY0AMIGJAoGBAIEW/uUSjcZqISYHVj1gJjFbgdWOpq/Z5kcnqx0CY25IscqXsZEEeODA44SehmBzLfvS3su5+7utNXE1IwNTVTtvSxPSfcaklDO1bAJOifbrQRAKMdm9zcACDvKWFdnpsHGq0f/BhYxuXFNd00w11TwiIi/fRViwV4EqWmAwJ9+PAgMBAAGjggFTMIIBTzAfBgNVHSMEGDAWgBT4Jg45QH6chvZArU9E+qmhpUOt+TBuBggrBgEFBQcBAQRiMGAwNgYIKwYBBQUHMAKGKmh0dHA6Ly9jZXJ0cy5laWQuYmVsZ2l1bS5iZS9iZWxnaXVtcnMyLmNydDAmBggrBgEFBQcwAYYaaHR0cDovL29jc3AuZWlkLmJlbGdpdW0uYmUwRAYDVR0gBD0wOzA5BgdgOAkBAQIBMC4wLAYIKwYBBQUHAgEWIGh0dHA6Ly9yZXBvc2l0b3J5LmVpZC5iZWxnaXVtLmJlMDkGA1UdHwQyMDAwLqAsoCqGKGh0dHA6Ly9jcmwuZWlkLmJlbGdpdW0uYmUvZWlkYzIwMTIwNC5jcmwwDgYDVR0PAQH/BAQDAgZAMBEGCWCGSAGG+EIBAQQEAwIFIDAYBggrBgEFBQcBAwQMMAowCAYGBACORgEBMA0GCSqGSIb3DQEBBQUAA4IBAQCD2ZNn1irgPGOs7SXkXyiFP+J2Swwl9bRFCTq44WhN1wSfSbglGd7WBVnBLhnuoasF5G8QY0Vt6LA9DYRKP2WYkvEfRADmJVBXOzQc/X4T/kGHddmHyk8uPKuKoHvu0fJ8l30WuioPjmNx4lDAlHyLw5lMmlEqGkNeKb+LaPsceULGfZsDdSb6Kzl6LABY5a4zhYz+fOoq3HnlxHbK9uJNb/yL2TgI57IFFg7/Rcpm/DDgZHknd++TPBE+W9/wQ5k/YzCk6bjW206c8tlrtkZIxwVCEQMzpDCJp8InoUqnlFxTXSLLTxdsLpU2riyxPuNC5Qes50XGy+WCOgGLygbb</eid:Certificate><eid:Certificate>MIID3jCCAsagAwIBAgIQc0D0rXT4VSFKh5sH3aRxDjANBgkqhkiG9w0BAQUFADAoMQswCQYDVQQGEwJCRTEZMBcGA1UEAxMQQmVsZ2l1bSBSb290IENBMjAeFw0xMTExMjMxMTAwMDBaFw0xODA3MjMxMTAwMDBaMDMxCzAJBgNVBAYTAkJFMRMwEQYDVQQDEwpDaXRpemVuIENBMQ8wDQYDVQQFEwYyMDEyMDQwggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQCXd7fTaijI+u7jXAjQxTw4PwHo3tuXLoZHYndqGAYVFfL3hj3cz0V7177WD3ZTxrqTDK8r8IxFogcQYwQZi6JzyT2HE+x9tgFy5OOKo/tecO/eeXX/hX6/QnmoJIQn2sMjdQo4TbOMJVyqIAziONU2Fv3J/p31ZPcGrBwB6DgMYlZ2/9CepREu3dHVcFG8NWai45rFE0M1UO3/zKoXOKTEZLTKR15jmfHMIcRvN5Yxj8VcrBCfyYaiBjGOc7wLPzJju7yaYnr+ZtzGfUpPTKGggyl6nDNJlY9EIFIFxmp4LUKlPWliGgChZM6FA1w2dunfyLEGHJ73eyqWjh2ikM6tAgMBAAGjgfgwgfUwDgYDVR0PAQH/BAQDAgEGMBIGA1UdEwEB/wQIMAYBAf8CAQAwQwYDVR0gBDwwOjA4BgZgOAkBAQIwLjAsBggrBgEFBQcCARYgaHR0cDovL3JlcG9zaXRvcnkuZWlkLmJlbGdpdW0uYmUwHQYDVR0OBBYEFPgmDjlAfpyG9kCtT0T6qaGlQ635MDcGA1UdHwQwMC4wLKAqoCiGJmh0dHA6Ly9jcmwuZWlkLmJlbGdpdW0uYmUvYmVsZ2l1bTIuY3JsMBEGCWCGSAGG+EIBAQQEAwIABzAfBgNVHSMEGDAWgBSFiuv0xbu+DlkDlN7WgAEV4xCcOTANBgkqhkiG9w0BAQUFAAOCAQEAlDk/FgLlMRzjn4Bw8CYhwi/qAyojus4cPOncKFa1PYlzHQNIabYBrll3TMp7cWEe4hnRy/rrCOsBQABZd0Qbbgnh95H5A5nRFmxGGW+OzQYmScWjrHdLIo+Ti+hPNUwoBjtreW53OEbiWHw3k3+HC+eUBLNBBu0HlqC8PWQDVxurxyHibLi+F9NzWiWa3EM7ANmNLAhU1uga3aQ2iiaXIRozIn13N8OHUEKfpS4ACW2nXb1AB4cMKJ+ACT6VD6ArUd9zGiz/D6F5BUbSH/2ZqtF6GHPx0fKET1ovdIUGZfZmOAZLZVC67IC4FlsYAklRyrCQZOTNxx1gFgn60ldzSA==</eid:Certificate><eid:Certificate>MIIDjjCCAnagAwIBAgIIKv++n6Lw6YcwDQYJKoZIhvcNAQEFBQAwKDELMAkGA1UEBhMCQkUxGTAXBgNVBAMTEEJlbGdpdW0gUm9vdCBDQTIwHhcNMDcxMDA0MTAwMDAwWhcNMjExMjE1MDgwMDAwWjAoMQswCQYDVQQGEwJCRTEZMBcGA1UEAxMQQmVsZ2l1bSBSb290IENBMjCCASIwDQYJKoZIhvcNAQEBBQADggEPADCCAQoCggEBAMZzQh6S/3UPi790hqc/7bIYLS2X+an7mEoj39WN4IzGMhwWLQdC1i22bi+n9fzGhYJdld61IgDMqFNAn68KNaJ6x+HK92AQZw6nUHMXU5WfIp8MXW+2QbyM69odRr2nlL/zGsvU+40OHjPIltfsjFPekx40HopQcSZYtF3CiInaYNKJIT/e1wEYNm7hLHADBGXvmAYrXR5i3FVr/mZkIV/4L+HXmymvb82fqgxG0YjFnaKVn6w/Fa7yYd/vw2uaItgscf1YHewApDgglVrH1Tdjuk+bqv5WRi5j2Qsj1Yr6tSPwiRuhFA0m2kHwOI8w7QUmecFLTqG4flVSOmlGhHUCAwEAAaOBuzCBuDAOBgNVHQ8BAf8EBAMCAQYwDwYDVR0TAQH/BAUwAwEB/zBCBgNVHSAEOzA5MDcGBWA4CQEBMC4wLAYIKwYBBQUHAgEWIGh0dHA6Ly9yZXBvc2l0b3J5LmVpZC5iZWxnaXVtLmJlMB0GA1UdDgQWBBSFiuv0xbu+DlkDlN7WgAEV4xCcOTARBglghkgBhvhCAQEEBAMCAAcwHwYDVR0jBBgwFoAUhYrr9MW7vg5ZA5Te1oABFeMQnDkwDQYJKoZIhvcNAQEFBQADggEBAFHYhd27V2/MoGy1oyCcUwnzSgEMdL8rs5qauhjyC4isHLMzr87lEwEnkoRYmhC598wUkmt0FoqW6FHvv/pKJaeJtmMrXZRY0c8RcrYeuTlBFk0pvDVTC9rejg7NqZV3JcqUWumyaa7YwBO+mPyWnIR/VRPmPIfjvCCkpDZoa01gZhz5v6yAlGYuuUGK02XThIAC71AdXkbc98m6tTR8KvPG2F9fVJ3bTc0R5/0UAoNmXsimABKgX77OFP67H6dh96tK8QYUn8pJQsKpvO2FsauBQeYNxUJpU4c5nUwfAA4+Bw11V0SoU7Q2dmSZ3G7rPUZuFF1eR1ONeE3gJ7uOhXY=</eid:Certificate></eid:CertificateChain></eid:SignatureResponse>", Encoding.UTF8)).Result;
            Assert.AreEqual(System.Net.HttpStatusCode.OK, rsp.StatusCode, "eid:SignatureResponse status");

            //Applet End
            rsp = client.GetAsync("https://www.e-contract.be/dss-ws/signed-chrome?message=Native%20host%20has%20exited").Result;
            Assert.AreEqual(System.Net.HttpStatusCode.Redirect, rsp.StatusCode, "pending request response status");
            Assert.AreEqual(new Uri("https://www.e-contract.be/dss-ws/signed.xhtml"), rsp.Headers.Location, "pending request response redirect location");

            //obtain response
            String signed = client.GetStringAsync("https://www.e-contract.be/dss-ws/signed.xhtml").Result;
            String searchkey = "<input type=\"hidden\" name=\"SignResponse\" value=\"";
            int startIndex = signed.IndexOf(searchkey) + searchkey.Length;
            int endIndex = signed.IndexOf('"', startIndex);
            return signed.Substring(startIndex, endIndex - startIndex);
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
            Assert.AreEqual(new DateTime(2019, 1, 23, 11, 0, 0, DateTimeKind.Utc), si.TimeStampValidity, "TimeStampValidity");
        }
    }
}
