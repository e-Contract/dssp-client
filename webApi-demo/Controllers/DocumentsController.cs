using dssp_demo.Models;
using dssp_demo.Services;
using EContract.Dssp.Client;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace dssp_demo.Controllers
{
    public class DocumentsController : ApiController
    {

        private Documents documents = new Documents();

        public ICollection<DocInfo> Get()
        {
            return documents.All;
        }

    }
}
