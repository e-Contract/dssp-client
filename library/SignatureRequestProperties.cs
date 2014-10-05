using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EContract.Dssp.Client
{
    /// <summary>
    /// Additional properties that will be added to the signature
    /// </summary>
    public class SignatureRequestProperties : SignatureProperties
    {
        /// <summary>
        /// Set to visualize the signature
        /// </summary>
        public VisibleSignatureProperties VisibleSignature { get; set; }
    }
}
