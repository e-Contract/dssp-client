using EContract.Dssp.Client;
using forms_demo.Properties;
using System;
using System.IO;
using System.Web.Hosting;
using System.Runtime.Serialization.Formatters.Binary;

namespace forms_demo
{
    public partial class Sign : System.Web.UI.Page
    {

        protected void Page_Load(object sender, EventArgs e)
        {
            Document document = new Document();
            document.MimeType = "application/pdf";
            document.Content = File.OpenRead(Path.Combine(HostingEnvironment.ApplicationPhysicalPath, @"App_Data\dssp-specs.pdf"));

            DsspClient dsspClient = new DsspClient("https://www.e-contract.be/dss-ws/dss");
            dsspClient.ApplicationName = Settings.Default.AppName;
            dsspClient.ApplicationPassword = Settings.Default.AppPwd;
            DsspSession dsspSession = dsspClient.UploadDocument(document);
            Session["dsspSession"] = dsspSession;

            VisibleSignatureProperties visibleSignature = null;
            if (Session["Visible"] == "Photo")
            {
                visibleSignature = new ImageVisibleSignature()
                {
                    Page = (int) Session["Page"],
                    X = (int) Session["X"],
                    Y = (int) Session["Y"]
                };
            } else if (Session["Visible"] == "Photo and Signer Info")
            {
                visibleSignature = new ImageVisibleSignature()
                {
                    Page = (int)Session["Page"],
                    X = (int)Session["X"],
                    Y = (int)Session["Y"],
                    ValueUri = "urn:be:e-contract:dssp:1.0:vs:si:eid-photo:signer-info",
                    CustomText = (string)Session["CustomText"]
                };
            }

            // verify whether DsspSession is serializable
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            MemoryStream memoryStream = new MemoryStream();
            binaryFormatter.Serialize(memoryStream, dsspSession);
            memoryStream.Seek(0, SeekOrigin.Begin);
            dsspSession = (DsspSession) binaryFormatter.Deserialize(memoryStream);

            this.PendingRequest.Value = dsspSession.GeneratePendingRequest(
                new Uri(Request.Url, ResolveUrl("~/Signed.aspx")),
                Settings.Default.Language,
                new SignatureRequestProperties() 
                {
                    SignerRole = (string)Session["Role"],
                    SignatureProductionPlace = (string)Session["Location"],
                    VisibleSignature = visibleSignature
                },
                Settings.Default.Authorization
            );
        }
    }
}