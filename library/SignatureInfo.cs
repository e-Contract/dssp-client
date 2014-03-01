/*
 *  This file is part of DSS-P client.
 *  Copyright (C) 2014 Egelke BVBA
 *  Copyright (C) 2014 e-contract BVBA
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
using System.Threading.Tasks;

namespace EContract.Dssp.Client
{
    public class SecurityInfo
    {
        public IList<SignatureInfo> Signatures { get; set; }

        /// <summary>
        /// The time before which the timestamp should be renewed.
        /// </summary>
        public DateTime TimeStampValidity { get; set; }
    }

    public class SignatureInfo
    {
        /// <summary>
        /// The certificate of the person that signed.
        /// </summary>
        public X509Certificate2 Signer { get; set; }

        /// <summary>
        /// The time at which the document was signed.
        /// </summary>
        public DateTime SigningTime { get; set; }

        
    }
}
