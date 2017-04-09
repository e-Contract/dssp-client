/*
 *  This file is part of DSS-P client.
 *  Copyright (C) 2017 Egelke BVBA
 *  Copyright (C) 2017 e-Contract.be BVBA
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
using System.Diagnostics;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace EContract.Dssp.Client
{
    /// <summary>
    /// Information linked to a single 2 step signature session.
    /// </summary>
    /// <remarks>
    /// A 2 step signature session consists of the following steps: upload document, sign localy and download document.
    /// See the e-contract.be documentation for more information.
    /// </remarks>
    [Serializable]
    public class Dssp2StepSession
    {
        /// <summary>
        /// The signer of the document
        /// </summary>
        public X509Certificate2 Signer { get; set; }

        /// <summary>
        /// The ID of the session
        /// </summary>
        public String CorrelationId { get; set; }

        /// <summary>
        /// The digest method that is used to obtain the digest value. 
        /// </summary>
        public String DigestAlgo { get; set; }

        /// <summary>
        /// The digest on which the signature must be calculated.
        /// </summary>
        public byte[] DigestValue { get; set; }

        /// <summary>
        /// The raw value of the signature
        /// </summary>
        public byte[] SignValue { get; set; }

        internal Dssp2StepSession()
        {

        }

        /// <summary>
        /// Calculates the signature
        /// </summary>
        /// <remarks>
        /// This step will trigger the OS to ask the PIN to the user via a popup.
        /// </remarks>
        public void Sign()
        {
            var key = (RSACryptoServiceProvider)Signer.PrivateKey;
            String digestOid = CryptoConfig.MapNameToOID(CryptoConfig.CreateFromName(DigestAlgo).GetType().ToString());
            SignValue = key.SignHash(DigestValue, digestOid);
        }
    }
}
