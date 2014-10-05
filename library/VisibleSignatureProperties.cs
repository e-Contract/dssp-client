using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EContract.Dssp.Client
{
    /// <summary>
    /// Base class to define a visible signature.
    /// </summary>
    public abstract class VisibleSignatureProperties
    {
        /// <summary>
        /// The page for visible signatures. Page starts at 1.
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// The x location for visible signatures.
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// The y location for visible signatures.
        /// </summary>
        public int Y { get; set; }
    }

    /// <summary>
    /// Visible signature that exist of the a photo (the eID photo).
    /// </summary>
    public class PhotoVisualSignature : VisibleSignatureProperties
    {
        /// <summary>
        /// The URI of the photo, defaults to the eID photo.
        /// </summary>
        /// <value>
        /// <c>urn:be:e-contract:dssp:1.0:vs:si:eid-photo</c> for eID photo (defaults).
        /// </value>
        public string ValueUri { get; set; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public PhotoVisualSignature()
        {
            ValueUri = "urn:be:e-contract:dssp:1.0:vs:si:eid-photo";
        }
    }
}
