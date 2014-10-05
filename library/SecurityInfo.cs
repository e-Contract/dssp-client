using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EContract.Dssp.Client
{
    /// <summary>
    /// Security information about the signatures of a document.
    /// </summary>
    public class SecurityInfo
    {
        /// <summary>
        /// Information about each and every signature that is present in the document.
        /// </summary>
        public IList<SignatureInfo> Signatures { get; set; }

        /// <summary>
        /// The time before which the timestamp should be renewed.
        /// </summary>
        public DateTime TimeStampValidity { get; set; }
    }
}
