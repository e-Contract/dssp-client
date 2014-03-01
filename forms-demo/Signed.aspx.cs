using EContract.Dssp.Client;
using EContract.Dssp.Client.Proxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace forms_demo
{
    public partial class Signed : System.Web.UI.Page
    {

        DsspClient dsspClient = new DsspClient("https://www.e-contract.be/dss-ws/dss");

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                //Retreive the content
                string signResponse = Request.Form.Get("SignResponse");

                //Retreive the session
                DsspSession session = (DsspSession)Session["dsspSession"];

                Document signedDocument;
                try
                {
                    //Check if the content is valid, this isn't required but stongly adviced.
                    NameIdentifierType newSigner = session.ValidateSignResponse(signResponse);

                    //Remove the DSS-P Session from the HTTP Session 
                    Session.Remove("dsspSession");

                    //download the signed document
                    signedDocument = dsspClient.DownloadDocumentAsync(session).Result;

                    //You should save the signed document about here...

                    //For demo purposes, lets validate the signature.  This is purely optional
                    SecurityInfo securityInfo = dsspClient.VerifyAsync(signedDocument).Result;

                    //Display some interesting info about the signed document
                    this.msg.Text = "signed document with timestamp valid until " + securityInfo.TimeStampValidity;
                    foreach(SignatureInfo signature in securityInfo.Signatures)
                    {
                        this.signatures.Items.Add("Singed by " + signature.Signer.Subject + " on " + signature.SigningTime);
                    }
                }
                catch (RequestError error)
                {
                    //Failed, lets display the error
                    this.msg.Text = "signing error: " + error.Message;
                    return;
                }
            }
        }

        protected void home_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/");
        }
    }
}