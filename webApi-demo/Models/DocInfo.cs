using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace dssp_demo.Models
{
    public class DocInfo
    {
        public DocInfo(string id, string name)
        {
            this.Id = id;
            this.Name = name;
        }

        public string Id;
        public string Name;
    }
}