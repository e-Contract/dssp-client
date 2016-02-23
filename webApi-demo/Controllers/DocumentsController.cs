using dssp_demo.Models;
using dssp_demo.Services;
using System.Collections.Generic;
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
