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
            Init();
        }

        public static void Init()
        {
            DocInfo doc;

            doc = new DocInfo("_1", "dssp-specs-0.9.0.pdf", "Digital Signature Service Protocol Specifications version 0.9.0");
            docs[doc.Id] = doc;
            doc = new DocInfo("_2", "dssp-specs-1.1.0.pdf", "Digital Signature Service Protocol Specifications version 1.1.0");
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