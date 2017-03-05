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
    [Serializable]
    public class Dssp2StepSession
    {
        public X509Certificate2 Signer { get; set; }

        public String CorrelationId { get; set; }

        public String DigestAlgo { get; set; }

        public byte[] DigestValue { get; set; }

        public byte[] SignValue { get; set; }

        internal Dssp2StepSession()
        {

        }

        public void Sign()
        {
            var key = (RSACryptoServiceProvider)Signer.PrivateKey;
            String digestOid = CryptoConfig.MapNameToOID(CryptoConfig.CreateFromName(DigestAlgo).GetType().ToString());
            SignValue = key.SignHash(DigestValue, digestOid);
        }
    }
}
