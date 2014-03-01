using EContract.Dssp.Client;
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
        DsspClient dsspClient = new DsspClient("https://www.e-contract.be/dss-ws/dss");

        protected void Page_Load(object sender, EventArgs e)
        {
            Document document = new Document();
            document.MimeType = "application/pdf";
            document.Content = File.OpenRead(Path.Combine(HostingEnvironment.ApplicationPhysicalPath, @"App_Data\dssp-specs.pdf"));

            DsspSession dsspSession = dsspClient.UploadDocument(document);

            Session["dsspSession"] = dsspSession;

            this.PendingRequest.Value = dsspSession.GeneratePendingRequest(new Uri(Request.Url, ResolveUrl("~/Signed.aspx")), "nl");
        }
    }
}