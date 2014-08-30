using dssp_demo.Models;
using dssp_demo.Services;
using EContract.Dssp.Client;
using EContract.Dssp.Client.Proxy;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Http;

namespace dssp_demo.Controllers
{
    [RoutePrefix("api/documents/{id}/signing")]
    public class SignController : ApiController
    {
        //The DSS-P client for e-contract
        DsspClient dsspClient = new DsspClient(new EndpointAddress("https://www.e-contract.be/dss-ws/dss"));

        //Reference to the documents
        Documents docs = new Documents();

        //Reference to the sessions
        DsspSessions sessions = new DsspSessions();

        public SignController()
        {
            dsspClient.ApplicationName = "egelke";
            dsspClient.ApplicationPassword = "egelke";
        }

        [Route("")]
        public async Task<HttpResponseMessage> Get(string id)
        {
            //get the requested document and covert it for upload.
            Document doc = docs[id].ToDocument();

            //Upload it, keeping the DSS-P session that is returned
            sessions[id] = await dsspClient.UploadDocumentAsync(doc);

            //creating the browser post page with the pending request
            string browserPostPage = sessions[id].GeneratePendingRequestPage(new Uri("https://www.e-contract.be/dss-ws/start"), Request.RequestUri);

            //returning it to the browser to execute
            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
            result.Content = new ByteArrayContent(Encoding.ASCII.GetBytes(browserPostPage));
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
            return result;
        }

        [Route("")]
        public async Task<HttpResponseMessage> Post(string id, [FromBody] FormDataCollection formData)
        {
            foreach(KeyValuePair<String, String> formField in formData)
            {
                if (formField.Key == "SignResponse")
                {
                    //check if the sign response is correct, keep the signer (currently always null)
                    NameIdentifierType newSigner = sessions[id].ValidateSignResponse(formField.Value);

                    //get the session and remove it from the store
                    DsspSession session = sessions.Remove(id);

                    //Download the signed document.
                    Document doc = await dsspClient.DownloadDocumentAsync(session);

                    //You should save the signed document about here...

                    //For demo purposes, lets validate the signature.  This is purely optional
                    SecurityInfo securityInfo = await dsspClient.VerifyAsync(doc);

                    //Keep some interesting info about the signed document
                    docs[id].TimeStampValidity = securityInfo.TimeStampValidity;
                    docs[id].NewSignatureBy = newSigner.Value;
                    docs[id].Signatures = new List<SignInfo>();
                    foreach (SignatureInfo info in securityInfo.Signatures)
                    {
                        SignInfo i = new SignInfo();
                        i.Signer = info.SignerSubject;
                        i.SignedOn = info.SigningTime;
                        docs[id].Signatures.Add(i);
                    }

                    //Redirecting back to the main site (via HTML to make sure "Get" is used instead of POST
                    var builder = new StringBuilder(); 
                    builder.AppendLine("<html>");
                    builder.AppendLine("<head><META HTTP-EQUIV=\"refresh\" CONTENT=\"0;URL=" + RequestContext.VirtualPathRoot + "/\"></head>");
                    builder.AppendLine("<body><a href=\"/\">done</a></body>");
                    builder.AppendLine("</html>");

                    HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
                    result.Content = new ByteArrayContent(Encoding.ASCII.GetBytes(builder.ToString()));
                    result.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
                    return result;
                }
            }
            return Request.CreateResponse(HttpStatusCode.BadRequest);
        }
    }
}
