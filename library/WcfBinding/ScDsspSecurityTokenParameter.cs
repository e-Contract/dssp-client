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
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.ServiceModel.Security.Tokens;

namespace EContract.Dssp.Client.WcfBinding
{
    internal class ScDsspSecurityTokenParameter : SecurityTokenParameters
    {

        protected override SecurityTokenParameters CloneCore()
        {
            return new ScDsspSecurityTokenParameter();
        }

        protected override void InitializeSecurityTokenRequirement(SecurityTokenRequirement requirement)
        {
            requirement.TokenType = ScDsspSecurityTokenProvider.TokenType;
        }

        protected override System.IdentityModel.Tokens.SecurityKeyIdentifierClause CreateKeyIdentifierClause(System.IdentityModel.Tokens.SecurityToken token, SecurityTokenReferenceStyle referenceStyle)
        {
            if (referenceStyle == SecurityTokenReferenceStyle.Internal)
            {
                return token.CreateKeyIdentifierClause<LocalIdKeyIdentifierClause>();
            }
            else
            {
                throw new NotSupportedException("External references are not supported for credit card tokens");
            }
        }

        protected override bool HasAsymmetricKey
        {
            get { return false; }
        }

        protected override bool SupportsClientAuthentication
        {
            get { return true; }
        }

        protected override bool SupportsClientWindowsIdentity
        {
            get { return false; }
        }

        protected override bool SupportsServerAuthentication
        {
            get { return true; }
        }
    }
}
