using dssp_demo.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace dssp_demo.Services
{
    public class Documents
    {
        //Demo only, this should be an actual document store of some sorts
        private static ConcurrentDictionary<String, DocInfo> docs = new ConcurrentDictionary<String, DocInfo>();

        static Documents()
        {
            var doc = new DocInfo("_1", "dssp-specs.pdf");
            docs[doc.Id] = doc;
        }

        public DocInfo[] All
        {
            get
            {
                return docs.Values.ToArray<DocInfo>();
            }
        }

        public DocInfo this[string id]
        {
            get
            {
                return docs[id];
            }
        }
    }
}