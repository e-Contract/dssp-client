/*
 *  This file is part of DSS-P client.
 *  Copyright (C) 2014 Egelke BVBA
 *  Copyright (C) 2014 e-Contract.be BVBA
 *
 *  DSS-P client is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU Lesser General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  DSS-P client is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Lesser General Public License for more details.
 *
 *  You should have received a copy of the GNU Lesser General Public License
 *  along with DSS-P client.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EContract.Dssp.Client
{
    /// <summary>
    /// Additional properties that will be added to the signature
    /// </summary>
    public class SignatureProperties
    {

        public SignatureProperties()
        {
            VisibleSignature = null;
        }

        /// <summary>
        /// The purported place where the signer claims to have produced the signature.
        /// </summary>
        public string SignatureProductionPlace { get; set; }

        /// <summary>
        /// Claimed or certified roles assumed by the signer in creating the signature.
        /// </summary>
        public string SignerRole { get; set; }

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

        /// <summary>
        /// The type of signature visualization.
        /// </summary>
        public VisibleSignatureType? VisibleSignature { get; set; }
    }

    /// <summary>
    /// Enumeration of the different supported signature visualizations.
    /// </summary>
    public enum VisibleSignatureType
    {
        /// <summary>
        /// eID Photo based signature visualization.
        /// </summary>
        Photo
    }
}
