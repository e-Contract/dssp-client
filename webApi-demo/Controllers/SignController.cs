using dssp_demo.Models;
using EContract.Dssp.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Http;

namespace dssp_demo.Controllers
{
    [RoutePrefix("api/documents/{id}/signing")]
    public class SignController : ApiController
    {

        DsspClient dsspClient = new DsspClient(null, new EndpointAddress("https://www.e-contract.be/dss-ws/dss"), DsspSessionMemoryStore.Default);

        [Route("")]
        public async Task<SignInfo> Post(string id)
        {
            dsspClient.LandingPage = new Uri(Request.RequestUri.ToString() + "/final");

            Document d = new Document();
            d.Id = id;
            d.MimeType = "application/pdf";
            d.Content = File.OpenRead(Path.Combine(HostingEnvironment.ApplicationPhysicalPath, @"App_Data\dssp-specs.pdf"));  //we should look up the document according to its id.
            d.Language = "fr";

            try
            {
                return new SignInfo(await dsspClient.UploadDocumentAsync(d));
            }
            catch (Exception e)
            {
                throw e;
            }
            
        }

        [Route("final")]
        public async Task<HttpResponseMessage> Post(string id, [FromBody] FormDataCollection formData)
        {
            foreach(KeyValuePair<String, String> formField in formData)
            {
                if (formField.Key == "SignResponse")
                {
                    byte[] signResponse = Convert.FromBase64String(formField.Value);

                    Document doc;

                    try
                    {
                        doc = await dsspClient.DownloadDocumentAsync(signResponse);
                        HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
                        result.Content = new StreamContent(doc.Content);
                        result.Content.Headers.ContentLength = doc.Content.Length;
                        result.Content.Headers.ContentType = new MediaTypeHeaderValue(doc.MimeType);
                        return result;
                    }
                    catch (Exception e)
                    {
                        throw e;
                    }
                }
            }
            return Request.CreateResponse(HttpStatusCode.BadRequest);
        }
    }
}
