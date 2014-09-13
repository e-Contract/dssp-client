using EContract.Dssp.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
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
        [IgnoreDataMember]
        public Stream Content;

        public bool Signed
        {
            get
            {
                return TimeStampValidity != null;
            }
        }

        public Document ToDocument()
        {
            Document d = new Document();
            d.MimeType = "application/pdf";
            if (this.Content == null)
            {
                this.Content = File.OpenRead(Path.Combine(HostingEnvironment.ApplicationPhysicalPath, @"App_Data\" + this.Name));
            }
            else
            {
                this.Content.Seek(0, SeekOrigin.Begin);
            }
            d.Content = this.Content;

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