using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EContract.Dssp.Client
{
    /// <summary>
    /// Contains the rules to determine who is authorized.
    /// </summary>
    public class Authorization
    {
        public static Authorization AllowDssSignIfMatchSubject(string subjectName)
        {
            return new Authorization()
            {
                Subjects = new Subject[1] { Subject.MatchSubject(subjectName) },
                Resource = "urn:be:e-contract:dss",
                Action = "sign"
            };
        }

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

        // <summary>
        /// Creates a subject that matches the "subject"-field of the signer (eID) certificate via a regular expression.
        /// </summary>
        /// <param name="subjectName">The regular expression to match against the subject of the signer certificate in DSS-P notation (not the same as .Net)</param>
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
