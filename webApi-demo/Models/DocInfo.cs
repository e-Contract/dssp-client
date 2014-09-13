using EContract.Dssp.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;

namespace dssp_demo.Models
{
    public class DocInfo
    {
        public DocInfo(string id, string name, string description)
        {
            this.Id = id;
            this.Name = name;
            this.Description = description;
        }

        public string Id;
        public string Name;
        public string Description;
        public Alert Alert;
        public DateTime? TimeStampValidity;
        public IList<SignInfo> Signatures;

        public bool Signed
        {
            get
            {
                return TimeStampValidity != null;
            }
        }

        public bool HasAlert
        {
            get
            {
                return Alert != null;
            }
        }

        public Document ToDocument()
        {
            Document d = new Document();
            d.MimeType = "application/pdf";
            d.Content = File.OpenRead(Path.Combine(HostingEnvironment.ApplicationPhysicalPath, @"App_Data\" + this.Name));

            return d;
        }
    }

    public class Alert
    {
        public string Message;
        public string Type;
    }

    public class SignInfo
    {
        public string Signer;
        public DateTime SignedOn;
        public string Location;
        public string Role;
    }
}