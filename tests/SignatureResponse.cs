using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace EContract.Dssp.Client
{
    [XmlRoot(ElementName = "SignatureResponse",
        Namespace = "urn:be:e-contract:eid:protocol:1.0.0")]
    public class SignatureResponse
    {
        
        public SignatureResponse()
        {
            CertificateChain = new List<byte[]>();
        }

        public byte[] SignatureValue { get; set; }

        [XmlArrayItem(ElementName = "Certificate")]
        public List<byte[]> CertificateChain { get; set; }
    }
}
