using EContract.Dssp.Client;
using forms_demo.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Web;
using System.Web.Hosting;
using System.Web.UI;
using System.Web.UI.WebControls;

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

            this.PendingRequest.Value = dsspSession.GeneratePendingRequest(new Uri(Request.Url, ResolveUrl("~/Signed.aspx")), Settings.Default.Language, 
                new SignatureProperties() { SignerRole = (string) Session["Role"], SignatureProductionPlace = (string) Session["Location"] },
                Settings.Default.Authorization);
        }
    }
}