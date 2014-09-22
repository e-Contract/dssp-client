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
using System.Security.Cryptography.X509Certificates;
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
    
    /// <summary>
    /// Security information about a single signature in a document.
    /// </summary>
    public class SignatureInfo : SignatureProperties
    {
        /// <summary>
        /// The certificate of the person that signed.
        /// </summary>
        public X509Certificate2 Signer { get; set; }

        /// <summary>
        /// The subject of the signer certificate (in DSS-P format)
        /// </summary>
        public string SignerSubject { get; set; }

        /// <summary>
        /// The time at which the document was signed.
        /// </summary>
        public DateTime SigningTime { get; set; }

        
    }
}
