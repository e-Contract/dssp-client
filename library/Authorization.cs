/*
 *  This file is part of DSS-P client.
 *  Copyright (C) 2014 Egelke BVBA
 *  Copyright (C) 2014-2016 e-Contract.be BVBA
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

namespace EContract.Dssp.Client
{
    /// <summary>
    /// Contains the rules to determine who is authorized.
    /// </summary>
    public class Authorization
    {
        private readonly List<string> authorizedSubjectNames = new List<string>();
        private readonly List<string> authorizedSubjectRegexps = new List<string>();
        private readonly List<string> authorizedCardNumbers = new List<string>();
        private readonly List<string> nonAuthorizedSubjectNames = new List<string>();
        private readonly List<string> nonAuthorizedSubjectRegexps = new List<string>();
        private readonly List<string> nonAuthorizedCardNumbers = new List<string>();

        /// <summary>
        /// Construct an authorization that authorizes signing with DSS if the eID subject matches exactly.
        /// </summary>
        /// <param name="subjectName">The subject to match exactly</param>
        /// <returns>The configured authorization object</returns>
        public static Authorization AllowDssSignIfMatchSubject(string subjectName)
        {
            Authorization authorization = new Authorization();
            authorization.authorizedSubjectNames.Add(subjectName);
            return authorization;
        }

        /// <summary>
        /// Construct an authorization that authorizes signing with DSS if the eID subject matches the regular expression.
        /// </summary>
        /// <param name="regex">The regular expression the subject must match</param>
        /// <returns>The configured authorization object</returns>
        public static Authorization AllowDssSignIfMatchSubjectRegex(string regex)
        {
            Authorization authorization = new Authorization();
            authorization.authorizedSubjectRegexps.Add(regex);
            return authorization;
        }

        public static Authorization DenyDssSignIfMatchSubject(string subjectName)
        {
            Authorization authorization = new Authorization();
            authorization.nonAuthorizedSubjectNames.Add(subjectName);
            return authorization;
        }

        public static Authorization DenyDssSignIfMatchSubjectRegex(string regex)
        {
            Authorization authorization = new Authorization();
            authorization.nonAuthorizedSubjectRegexps.Add(regex);
            return authorization;
        }

        public void AddAuthorizedSubjectName(string subjectName)
        {
            this.authorizedSubjectNames.Add(subjectName);
        }

        public void AddAuthorizedSubjectRegExp(string regexp)
        {
            this.authorizedSubjectRegexps.Add(regexp);
        }

        public void AddAuthorizedCardNumber(string cardNumber)
        {
            this.authorizedCardNumbers.Add(cardNumber);
        }

        public void AddNonAuthorizedSubjectName(string subjectName)
        {
            this.nonAuthorizedSubjectNames.Add(subjectName);
        }

        public void AddNonAuthorizedSubjectRegExp(string regexp)
        {
            this.nonAuthorizedSubjectRegexps.Add(regexp);
        }

        public void AddNonAuthorizedCardNumber(string cardNumber)
        {
            this.nonAuthorizedCardNumbers.Add(cardNumber);
        }

        /// <summary>
        /// Gives back the XACML policy corresponding with the configured signature authorization.
        /// </summary>
        /// <returns>The XAML policy object.</returns>
        public PolicyType getPolicy()
        {
            PolicyType policy = new PolicyType();

            policy.PolicyId = "urn:egelke:dssp:pendingrequest:policy";
            policy.RuleCombiningAlgId = "urn:oasis:names:tc:xacml:1.0:rule-combining-algorithm:deny-overrides";

            {
                policy.Target = new TargetType();
                policy.Target.Resources = new ResourceMatchType[1][];
                policy.Target.Resources[0] = new ResourceMatchType[1];
                policy.Target.Resources[0][0] = new ResourceMatchType();
                policy.Target.Resources[0][0].MatchId = "urn:oasis:names:tc:xacml:1.0:function:anyURI-equal";
                policy.Target.Resources[0][0].AttributeValue = new AttributeValueType();
                policy.Target.Resources[0][0].AttributeValue.Value = "urn:be:e-contract:dss";
                policy.Target.Resources[0][0].AttributeValue.DataType = "http://www.w3.org/2001/XMLSchema#anyURI";
                policy.Target.Resources[0][0].ResourceAttributeDesignator = new AttributeDesignatorType();
                policy.Target.Resources[0][0].ResourceAttributeDesignator.AttributeId = "urn:oasis:names:tc:xacml:1.0:resource:resource-id";
                policy.Target.Resources[0][0].ResourceAttributeDesignator.DataType = "http://www.w3.org/2001/XMLSchema#anyURI";
                policy.Target.Actions = new ActionMatchType[1][];
                policy.Target.Actions[0] = new ActionMatchType[1];
                policy.Target.Actions[0][0] = new ActionMatchType();
                policy.Target.Actions[0][0].MatchId = "urn:oasis:names:tc:xacml:1.0:function:string-equal";
                policy.Target.Actions[0][0].AttributeValue = new AttributeValueType();
                policy.Target.Actions[0][0].AttributeValue.Value = "sign";
                policy.Target.Actions[0][0].AttributeValue.DataType = "http://www.w3.org/2001/XMLSchema#string";
                policy.Target.Actions[0][0].ActionAttributeDesignator = new AttributeDesignatorType();
                policy.Target.Actions[0][0].ActionAttributeDesignator.AttributeId = "urn:oasis:names:tc:xacml:1.0:action:action-id";
                policy.Target.Actions[0][0].ActionAttributeDesignator.DataType = "http://www.w3.org/2001/XMLSchema#string";
            }

            if (this.nonAuthorizedSubjectNames.Count != 0 || this.nonAuthorizedSubjectRegexps.Count != 0 ||this.nonAuthorizedCardNumbers.Count != 0)
            {
                policy.Rule = new RuleType[2];
                policy.Rule[1] = new RuleType();
                policy.Rule[1].RuleId = "deny-subject";
                policy.Rule[1].Effect = EffectType.Deny;
                policy.Rule[1].Target = new TargetType();
                List<SubjectMatchType> subjects = new List<SubjectMatchType>();
                foreach(string nonAuthorizedSubjectName in this.nonAuthorizedSubjectNames)
                {
                    var subjectMatch = new SubjectMatchType();
                    subjectMatch.MatchId = "urn:oasis:names:tc:xacml:1.0:function:x500Name-equal";
                    subjectMatch.AttributeValue = new AttributeValueType();
                    subjectMatch.AttributeValue.DataType = "urn:oasis:names:tc:xacml:1.0:data-type:x500Name";
                    subjectMatch.AttributeValue.Value = nonAuthorizedSubjectName;
                    subjectMatch.SubjectAttributeDesignator = new SubjectAttributeDesignatorType();
                    subjectMatch.SubjectAttributeDesignator.AttributeId = "urn:oasis:names:tc:xacml:1.0:subject:subject-id";
                    subjectMatch.SubjectAttributeDesignator.DataType = "urn:oasis:names:tc:xacml:1.0:data-type:x500Name";
                    subjects.Add(subjectMatch);
                }
                foreach(string nonAuthorizedSubjectRegexp in this.nonAuthorizedSubjectRegexps)
                {
                    var subjectMatch = new SubjectMatchType();
                    subjectMatch.MatchId = "urn:oasis:names:tc:xacml:2.0:function:x500Name-regexp-match";
                    subjectMatch.AttributeValue = new AttributeValueType();
                    subjectMatch.AttributeValue.DataType = "http://www.w3.org/2001/XMLSchema#string";
                    subjectMatch.AttributeValue.Value = nonAuthorizedSubjectRegexp;
                    subjectMatch.SubjectAttributeDesignator = new SubjectAttributeDesignatorType();
                    subjectMatch.SubjectAttributeDesignator.AttributeId = "urn:oasis:names:tc:xacml:1.0:subject:subject-id";
                    subjectMatch.SubjectAttributeDesignator.DataType = "urn:oasis:names:tc:xacml:1.0:data-type:x500Name";
                    subjects.Add(subjectMatch);
                }
                foreach (string nonAuthorizedCardNumber in this.nonAuthorizedCardNumbers)
                {
                    var subjectMatch = new SubjectMatchType();
                    subjectMatch.MatchId = "urn:oasis:names:tc:xacml:1.0:function:string-equal";
                    subjectMatch.AttributeValue = new AttributeValueType();
                    subjectMatch.AttributeValue.DataType = "http://www.w3.org/2001/XMLSchema#string";
                    subjectMatch.AttributeValue.Value = nonAuthorizedCardNumber;
                    subjectMatch.SubjectAttributeDesignator = new SubjectAttributeDesignatorType();
                    subjectMatch.SubjectAttributeDesignator.AttributeId = "urn:be:e-contract:dss:eid:card-number";
                    subjectMatch.SubjectAttributeDesignator.DataType = "http://www.w3.org/2001/XMLSchema#string";
                    subjects.Add(subjectMatch);
                }
                policy.Rule[1].Target.Subjects = new SubjectMatchType[subjects.Count][];
                for (int i = 0; i < subjects.Count; i++)
                {
                    policy.Rule[1].Target.Subjects[i] = new SubjectMatchType[1];
                    policy.Rule[1].Target.Subjects[i][0] = subjects[i];
                }
            } else
            {
                policy.Rule = new RuleType[1];
            }

            policy.Rule[0] = new RuleType();
            policy.Rule[0].RuleId = "permit-subject";
            policy.Rule[0].Effect = EffectType.Permit;

            if (this.authorizedSubjectNames.Count != 0 || this.authorizedSubjectRegexps.Count != 0 || this.authorizedCardNumbers.Count != 0)
            {
                policy.Rule[0].Target = new TargetType();
                List<SubjectMatchType> subjects = new List<SubjectMatchType>();
                foreach (string authorizedSubjectName in this.authorizedSubjectNames)
                {
                    var subjectMatch = new SubjectMatchType();
                    subjectMatch.MatchId = "urn:oasis:names:tc:xacml:1.0:function:x500Name-equal";
                    subjectMatch.AttributeValue = new AttributeValueType();
                    subjectMatch.AttributeValue.DataType = "urn:oasis:names:tc:xacml:1.0:data-type:x500Name";
                    subjectMatch.AttributeValue.Value = authorizedSubjectName;
                    subjectMatch.SubjectAttributeDesignator = new SubjectAttributeDesignatorType();
                    subjectMatch.SubjectAttributeDesignator.AttributeId = "urn:oasis:names:tc:xacml:1.0:subject:subject-id";
                    subjectMatch.SubjectAttributeDesignator.DataType = "urn:oasis:names:tc:xacml:1.0:data-type:x500Name";
                    subjects.Add(subjectMatch);
                }
                foreach (string authorizedSubjectRegexp in this.authorizedSubjectRegexps)
                {
                    var subjectMatch = new SubjectMatchType();
                    subjectMatch.MatchId = "urn:oasis:names:tc:xacml:2.0:function:x500Name-regexp-match";
                    subjectMatch.AttributeValue = new AttributeValueType();
                    subjectMatch.AttributeValue.DataType = "http://www.w3.org/2001/XMLSchema#string";
                    subjectMatch.AttributeValue.Value = authorizedSubjectRegexp;
                    subjectMatch.SubjectAttributeDesignator = new SubjectAttributeDesignatorType();
                    subjectMatch.SubjectAttributeDesignator.AttributeId = "urn:oasis:names:tc:xacml:1.0:subject:subject-id";
                    subjectMatch.SubjectAttributeDesignator.DataType = "urn:oasis:names:tc:xacml:1.0:data-type:x500Name";
                    subjects.Add(subjectMatch);
                }
                foreach (string authorizedCardNumber in this.authorizedCardNumbers)
                {
                    var subjectMatch = new SubjectMatchType();
                    subjectMatch.MatchId = "urn:oasis:names:tc:xacml:1.0:function:string-equal";
                    subjectMatch.AttributeValue = new AttributeValueType();
                    subjectMatch.AttributeValue.DataType = "http://www.w3.org/2001/XMLSchema#string";
                    subjectMatch.AttributeValue.Value = authorizedCardNumber;
                    subjectMatch.SubjectAttributeDesignator = new SubjectAttributeDesignatorType();
                    subjectMatch.SubjectAttributeDesignator.AttributeId = "urn:be:e-contract:dss:eid:card-number";
                    subjectMatch.SubjectAttributeDesignator.DataType = "http://www.w3.org/2001/XMLSchema#string";
                    subjects.Add(subjectMatch);
                }
                policy.Rule[0].Target.Subjects = new SubjectMatchType[subjects.Count][];
                for (int i = 0; i < subjects.Count; i++)
                {
                    policy.Rule[0].Target.Subjects[i] = new SubjectMatchType[1];
                    policy.Rule[0].Target.Subjects[i][0] = subjects[i];
                }
            }

            return policy;
        }
    }
}
