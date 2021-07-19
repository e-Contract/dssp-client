using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace EContract.Dssp.Client
{
    [XmlRoot(ElementName = "IdentificationResponse",
        Namespace = "urn:be:e-contract:eid:protocol:1.0.0")]
    public class IdentificationResponse
    {

        public IdentificationResponse()
        {
            CertificateChain = new List<byte[]>();
            SigningCertificateChain = new List<byte[]>();
        }

        [XmlArrayItem(ElementName = "Certificate")]
        public List<byte[]> CertificateChain { get; }

        public byte[] IdentityFile { get; set; }

        public byte[] IdentitySignatureFile { get; set; }

        public byte[] Photo { get; set; }

        [XmlArrayItem(ElementName = "Certificate")]
        public List<byte[]> SigningCertificateChain { get; }
    }
}
