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
    /// Contains the rules to determine who is authorized.
    /// </summary>
    public class Authorization
    {
        /// <summary>
        /// Construct an authorization that authorizes signing with DSS if the eID subject matches exactly.
        /// </summary>
        /// <param name="subjectName">The subject to match exactly</param>
        /// <returns>The configured authorization object</returns>
        public static Authorization AllowDssSignIfMatchSubject(string subjectName)
        {
            return new Authorization()
            {
                Subjects = new Subject[1] { Subject.MatchSubject(subjectName) },
                Resource = "urn:be:e-contract:dss",
                Action = "sign"
            };
        }

        /// <summary>
        /// Construct an authorization that authorizes signing with DSS if the eID subject matches the regular expression.
        /// </summary>
        /// <param name="regex">The regular expression the subject must match</param>
        /// <returns>The configured authorization object</returns>
        public static Authorization AllowDssSignIfMatchSubjectRegex(string regex)
        {
            return new Authorization()
            {
                Subjects = new Subject[1] { Subject.MatchSubjectRegex(regex) },
                Resource = "urn:be:e-contract:dss",
                Action = "sign"
            };
        }

        /// <summary>
        /// The allowed subject.
        /// </summary>
        public Subject[] Subjects { get; set; }

        /// <summary>
        /// The resource it applies to (e.g. "urn:be:e-contract:dss")
        /// </summary>
        public String Resource { get; set; }

        /// <summary>
        /// The action is applies to (e.g. "sign")
        /// </summary>
        public String Action { get; set; }
    }

    /// <summary>
    /// Defines how to check the subject.
    /// </summary>
    public class Subject
    {
        /// <summary>
        /// Creates a subject that exactly matches the "subject"-field of the signer (eID) certificate.
        /// </summary>
        /// <param name="subjectName">The subject of the signer certificate in DSS-P notation (not the same as .Net)</param>
        /// <returns>DSS-P compliant object</returns>
        public static Subject MatchSubject(string subjectName)
        {
            if (subjectName == null) throw new ArgumentNullException("subjectName");

            return new Subject()
            {
                MatchType = "urn:oasis:names:tc:xacml:1.0:function:x500Name-equal",
                MatchValue = subjectName,
                MatchDataType = "urn:oasis:names:tc:xacml:1.0:data-type:x500Name",
                AttributeId = "urn:oasis:names:tc:xacml:1.0:subject:subject-id",
                AttributeDataType = "urn:oasis:names:tc:xacml:1.0:data-type:x500Name"
            };
        }

        /// <summary>
        /// Creates a subject that matches the "subject"-field of the signer (eID) certificate via a regular expression.
        /// </summary>
        /// <param name="regex">The regular expression to match against the subject of the signer certificate in DSS-P notation (not the same as .Net)</param>
        /// <returns>DSS-P compliant object</returns>
        public static Subject MatchSubjectRegex(string regex)
        {
            if (regex == null) throw new ArgumentNullException("regex");

            return new Subject()
            {
                MatchType = "urn:oasis:names:tc:xacml:2.0:function:x500Name-regexp-match",
                MatchValue = regex,
                MatchDataType = "http://www.w3.org/2001/XMLSchema#string",
                AttributeId = "urn:oasis:names:tc:xacml:1.0:subject:subject-id",
                AttributeDataType = "urn:oasis:names:tc:xacml:1.0:data-type:x500Name"
            };
        }

        /// <summary>
        /// How must the attribute value be matched? Equals the DSS-P "xacmlp:SubjectMatch/@MatchId" value.
        /// </summary>
        public string MatchType {get; set;}

        /// <summary>
        /// Which attribute value of the subject must be checked?
        /// Equals the DSS-P "xacmlp:SubjectMatch/xacmlp:SubjectAttributeDesignator/@AttributeId" value.
        /// </summary>
        public string AttributeId { get ; set; }

        /// <summary>
        /// What is the data type of the attribute value that must be checked?
        /// Equals the DSS-P "xacmlp:SubjectMatch/xacmlp:SubjectAttributeDesignator/@AttributeId" value.
        /// </summary>
        public string AttributeDataType { get; set; }

        /// <summary>
        /// Against which value must the specified attribute match?
        /// Equals the DSS-P "xacmlp:SubjectMatch/xacmlp:AttributeValue/text()" value.
        /// </summary>
        public string MatchValue { get; set; }

        /// <summary>
        /// What is the data type of the match value?
        /// Equals the DSS-P "xacmlp:SubjectMatch/xacmlp:AttributeValue/@DataType" value.
        /// </summary>
        public string MatchDataType { get; set; }
    }
}
