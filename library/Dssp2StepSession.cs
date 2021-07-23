/*
 *  This file is part of DSS-P client.
 *  Copyright (C) 2017-2021 Egelke BVBA
 *  Copyright (C) 2017-2021 e-Contract.be BVBA
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

using Org.BouncyCastle.Asn1;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
#if NET461_OR_GREATER
            RSA rsaKey;
            ECDsa ecDsaKey;
            if ((ecDsaKey = Signer.GetECDsaPrivateKey()) != null)
            {
                SignValue = Ieee1363ToDer(ecDsaKey.SignHash(DigestValue));
            }
            else if ((rsaKey = Signer.GetRSAPrivateKey()) != null)
            {
                HashAlgorithmName hashAlgorithmName;
                HashAlgorithm hashAlgorithm = (HashAlgorithm) CryptoConfig.CreateFromName(DigestAlgo);

                if (hashAlgorithm is SHA1) hashAlgorithmName = HashAlgorithmName.SHA1;
                else if (hashAlgorithm is SHA256) hashAlgorithmName = HashAlgorithmName.SHA256;
                else if (hashAlgorithm is SHA384) hashAlgorithmName = HashAlgorithmName.SHA384;
                else if (hashAlgorithm is SHA512) hashAlgorithmName = HashAlgorithmName.SHA512;
                else throw new InvalidOperationException("Digest algo not supported");


                SignValue = rsaKey.SignHash(DigestValue, hashAlgorithmName, RSASignaturePadding.Pkcs1);
            } 
            else
            {
                throw new InvalidOperationException("Key type not supported");
            }
#else
            var key = (RSACryptoServiceProvider)Signer.PrivateKey;
            String digestOid = CryptoConfig.MapNameToOID(CryptoConfig.CreateFromName(DigestAlgo).GetType().ToString());
            SignValue = key.SignHash(DigestValue, digestOid);
#endif
        }

        private static byte[] Ieee1363ToDer(byte[] input)
        {
            // Input is (r, s), each of them exactly half of the array.
            // Output is the DER encoded value of SEQUENCE(INTEGER(r), INTEGER(s)).
            int halfLength = input.Length / 2;

            MemoryStream encoded = new MemoryStream();
            DerSequenceGenerator generator = new DerSequenceGenerator(encoded);
            generator.AddObject(Ieee1363KeyParameterIntegerToDer(input, 0, halfLength)); //add r
            generator.AddObject(Ieee1363KeyParameterIntegerToDer(input, halfLength, halfLength)); //add s
            generator.Close();

            return encoded.ToArray();
        }

        private static DerInteger Ieee1363KeyParameterIntegerToDer(byte[] paddedInt, int offset, int length)
        {
            int padding = 0;
            while (padding < paddedInt.Length && paddedInt[offset + padding] == 0) padding++;

            if (padding == paddedInt.Length) // all 0, we have the number 0
                new DerInteger(0);

            //false negative, so we need to add 1 more byte in front.
            int extra = paddedInt[offset + padding] >= 0x80 ? 1 : 0;

            byte[] integer = new byte[length - padding + extra];
            Array.Copy(paddedInt, offset + padding, integer, extra, length - padding);

            return new DerInteger(integer);
        }
    }
}
