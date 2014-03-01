using dssp_demo.Models;
using EContract.Dssp.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Web.Http;

namespace dssp_demo.Controllers
{
    public class DocumentsController : ApiController
    {

        

        public DocInfo[] Get()
        {
            return new DocInfo[] { new DocInfo("_1", "dssp-specs.pdf") };
        }

        
    }
}
