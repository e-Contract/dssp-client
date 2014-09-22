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

using EContract.Dssp.Client.Proxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EContract.Dssp.Client
{
    /// <summary>
    /// Requester authorization error.
    /// The signer was not authorized.
    /// </summary>
    public class AuthorizationError : RequestError
    {
        public NameIdentifierType AttemptedSigner { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">The exception message</param>
        public AuthorizationError(String message, NameIdentifierType attemptedSigner)
            : base(message)
        {
            this.AttemptedSigner = attemptedSigner;
        }
    }
}
