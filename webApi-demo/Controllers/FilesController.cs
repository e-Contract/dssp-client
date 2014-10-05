using dssp_demo.Services;
using EContract.Dssp.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;

namespace dssp_demo.Controllers
{
    public class FilesController : ApiController
    {
        private Documents documents = new Documents();

        public HttpResponseMessage Get(string id)
        {
            Document d = documents[id].ToDocument();

            var result = new HttpResponseMessage(HttpStatusCode.OK);
            result.Content = new MyStreamContent(d.Content);
            result.Content.Headers.ContentType = new MediaTypeHeaderValue(d.MimeType);
            return result;
        }
    }

    internal class MyStreamContent : StreamContent
    {
        public MyStreamContent(Stream stream)
            : base(stream)
        {

        }

        protected override void Dispose(bool disposing)
        {
            //We don't want to close the stream.
        }
    }
}
