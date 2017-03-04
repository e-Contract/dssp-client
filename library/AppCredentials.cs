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
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace EContract.Dssp.Client
{
    /// <summary>
    /// Contains the credentials of the application.
    /// </summary>
    public class AppCredentials
    {
        /// <summary>
        /// Credentials in the form of username and password.
        /// </summary>
        public UTCredentials UT { get; }

        /// <summary>
        /// Credentials in the form of certificate.
        /// </summary>
        public X509Credentials X509 { get; }

        //TODO:SAML-HOK

        /// <summary>
        /// Default constructor.
        /// </summary>
        public AppCredentials()
        {
            UT = new UTCredentials();
            X509 = new X509Credentials();
        }

    }

    /// <summary>
    /// Credentials in the form of username and password.
    /// </summary>
    public class UTCredentials
    {
        /// <summary>
        /// The username
        /// </summary>
        public String Name { get; set; }

        /// <summary>
        /// The password.
        /// </summary>
        public String Password { get; set; }
    }

    /// <summary>
    /// Credentials in the form of certificate, provided by direct reference or search params.
    /// </summary>
    public class X509Credentials
    {
        /// <summary>
        /// Direct reference to certificate
        /// </summary>
        public X509Certificate2 Certificate { get; set; }

        /// <summary>
        /// The store location to search the Certificate in.
        /// </summary>
        public StoreLocation StoreLocation { get; set; }
            
        /// <summary>
        /// The store name to search the Certificate in, defaults to local machine.
        /// </summary>
        public StoreName StoreName { get; set; }

        /// <summary>
        /// The type of Certificate search, defaults to My.
        /// </summary>
        public X509FindType FindType { get; set; }

        /// <summary>
        /// The value to search for, defaults to Subject Name
        /// </summary>
        public object FindValue { get; set; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public X509Credentials()
        {
            StoreLocation = StoreLocation.LocalMachine;
            StoreName = StoreName.My;
            FindType = X509FindType.FindBySubjectName;
        }
    }
}
