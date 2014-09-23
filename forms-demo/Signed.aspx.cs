using EContract.Dssp.Client;
using EContract.Dssp.Client.Proxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;

namespace forms_demo
{
    public partial class Signed : System.Web.UI.Page
    {

        DsspClient dsspClient = new DsspClient("https://www.e-contract.be/dss-ws/dss");

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                //Retrieve the content
                string signResponse = Request.Form.Get("SignResponse");

                //Retrieve the session
                DsspSession session = (DsspSession)Session["dsspSession"];

                Document signedDocument;
                try
                {
                    //Check if the content is valid, this isn't required but strongly advised.
                    NameIdentifierType newSigner = session.ValidateSignResponse(signResponse);

                    //Remove the DSS-P Session from the HTTP Session 
                    Session.Remove("dsspSession");

                    //download the signed document
                    signedDocument = dsspClient.DownloadDocument(session);

                    //You should save the signed document about here...
                    Session["signedDocument"] = signedDocument;

                    //For demo purposes, lets validate the signature.  This is purely optional
                    SecurityInfo securityInfo = dsspClient.Verify(signedDocument);

                    //Display some interesting info about the signed document
                    this.msg.Text = "signed document with timestamp valid until " + securityInfo.TimeStampValidity;
                    foreach(SignatureInfo signature in securityInfo.Signatures)
                    {
                        if (signature.SignerSubject == newSigner.Value)
                        {
                            this.signatures.Items.Add("New: Signed by " + signature.Signer.Subject + " on " + signature.SigningTime);
                        }
                        else
                        {
                            this.signatures.Items.Add("Signed by " + signature.Signer.Subject + " on " + signature.SigningTime);
                        }
                    }

                    this.view.Enabled = true;
                }
                catch (RequestError error)
                {
                    //Failed, lets display the error
                    this.msg.Text = "signing error: " + error.Message;
                    this.view.Enabled = false;
                    return;
                }
            }
        }

        protected void home_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/");
        }

        protected void view_Click(object sender, EventArgs e)
        {
            Document signedDocument = (Document) Session["signedDocument"];
            Response.ContentType = signedDocument.MimeType;
            CopyStream(signedDocument.Content, Response.OutputStream);
        }

        private static void CopyStream(Stream input, Stream output)
        {
            byte[] buffer = new byte[32768];
            int read;
            while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, read);
            }
        }
    }
}