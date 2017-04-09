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
using System.Linq;

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
            authorization.AddAuthorizedSubjectName(subjectName);
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
            authorization.AddAuthorizedSubjectRegExp(regex);
            return authorization;
        }

        /// <summary>
        /// Construct an authorization that authorizes signing with DSS if the eID Card matches the provided nummer.
        /// </summary>
        /// <param name="card">The card number that must be matched without seperators (only numbers)</param>
        /// <returns>The configured authorization object</returns>
        public static Authorization AllowDssSignIfMatchCardNumber(string card)
        {
            Authorization authorization = new Authorization();
            authorization.AddAuthorizedCardNumber(card);
            return authorization;
        }

        /// <summary>
        /// Construct an authorization that authorizes signing with DSS if the eID subject does NOT match exactly.
        /// </summary>
        /// <param name="subjectName">The subject to NOT match exactly</param>
        /// <returns>The configured authorization object</returns>
        public static Authorization DenyDssSignIfMatchSubject(string subjectName)
        {
            Authorization authorization = new Authorization();
            authorization.AddNonAuthorizedSubjectName(subjectName);
            return authorization;
        }

        /// <summary>
        /// Construct an authorization that authorizes signing with DSS if the eID subject does NOT match the regular expression.
        /// </summary>
        /// <param name="regex">The regular expression the subject must NOT match</param>
        /// <returns>The configured authorization object</returns>
        public static Authorization DenyDssSignIfMatchSubjectRegex(string regex)
        {
            Authorization authorization = new Authorization();
            authorization.AddNonAuthorizedSubjectRegExp(regex);
            return authorization;
        }

        /// <summary>
        /// Construct an authorization that authorizes signing with DSS if the eID Card does NOT match the provided nummer.
        /// </summary>
        /// <param name="card">The card number that must NOT be matched without seperators (only numbers)</param>
        /// <returns>The configured authorization object</returns>
        public static Authorization DenyDssSignIfMatchCardNumber(string card)
        {
            Authorization authorization = new Authorization();
            authorization.AddNonAuthorizedCardNumber(card);
            return authorization;
        }

        /// <summary>
        /// Add (additional) subject name to be machted exactly.
        /// </summary>
        /// <param name="subjectName">The subject to be matched exactly</param>
        public void AddAuthorizedSubjectName(string subjectName)
        {
            this.authorizedSubjectNames.Add(subjectName);
        }

        /// <summary>
        /// Add (additional) subject name to be machted via regular expression.
        /// </summary>
        /// <param name="regexp">The subject to be matched via regular expression</param>
        public void AddAuthorizedSubjectRegExp(string regexp)
        {
            this.authorizedSubjectRegexps.Add(regexp);
        }

        /// <summary>
        /// The (additional) card number to match exactly
        /// </summary>
        /// <param name="cardNumber">The card number that must be matched without seperators (only numbers)</param>
        public void AddAuthorizedCardNumber(string cardNumber)
        {
            this.authorizedCardNumbers.Add(cardNumber);
        }

        /// <summary>
        /// Add (additional) subject name to NOT macht exactly.
        /// </summary>
        /// <param name="subjectName">The subject to NOT match exactly</param>
        public void AddNonAuthorizedSubjectName(string subjectName)
        {
            this.nonAuthorizedSubjectNames.Add(subjectName);
        }

        /// <summary>
        /// Add (additional) subject name to NOT macht via regular expression.
        /// </summary>
        /// <param name="regexp">The subject to MOT match via regular expression</param>
        public void AddNonAuthorizedSubjectRegExp(string regexp)
        {
            this.nonAuthorizedSubjectRegexps.Add(regexp);
        }

        /// <summary>
        /// The (additional) card number to NOT match exactly
        /// </summary>
        /// <param name="cardNumber">The card number that must NOT be matched without seperators (only numbers)</param>
        public void AddNonAuthorizedCardNumber(string cardNumber)
        {
            this.nonAuthorizedCardNumbers.Add(cardNumber);
        }

        internal PolicyType Policy
        {
            get
            {
                SubjectMatchType[] allowSubjects = authorizedSubjectNames.Select(x => x.ToSubjectX500NameMatch())
                    .Concat(authorizedSubjectRegexps.Select(x => x.ToSubjectX500NameRegExMatch()))
                    .Concat(authorizedCardNumbers.Select(x => x.ToEidCardNrMatch()))
                    .ToArray();
                SubjectMatchType[] denySubjects = nonAuthorizedSubjectNames.Select(x => x.ToSubjectX500NameMatch())
                    .Concat(nonAuthorizedSubjectRegexps.Select(x => x.ToSubjectX500NameRegExMatch()))
                    .Concat(nonAuthorizedCardNumbers.Select(x => x.ToEidCardNrMatch()))
                    .ToArray();

                RuleType[] rules;
                if (allowSubjects.Length == 0 && denySubjects.Length == 0) {
                    return null;
                }
                else if (denySubjects.Length == 0)
                {
                    rules = new RuleType[] { allowSubjects.ToSubjectRule(EffectType.Permit) };
                }
                else if (allowSubjects.Length == 0)
                {
                    RuleType allowedRule = new RuleType();
                    allowedRule.RuleId = "allow-all-rule";
                    allowedRule.Effect = EffectType.Permit;
                    rules = new RuleType[] { allowedRule, denySubjects.ToSubjectRule(EffectType.Deny) };
                }
                else
                {
                    rules = new RuleType[] { allowSubjects.ToSubjectRule(EffectType.Permit), denySubjects.ToSubjectRule(EffectType.Deny) };
                }

                return new PolicyType()
                {
                    PolicyId = "urn:egelke:dssp:pendingrequest:policy",
                    RuleCombiningAlgId = "urn:oasis:names:tc:xacml:1.0:rule-combining-algorithm:deny-overrides",
                    Target = new TargetType()
                    {
                        Resources = new ResourceMatchType[][]
                        {
                            new ResourceMatchType[]
                            {
                                new ResourceMatchType()
                                {
                                    MatchId = "urn:oasis:names:tc:xacml:1.0:function:anyURI-equal",
                                    AttributeValue = new AttributeValueType()
                                    {
                                        DataType = "http://www.w3.org/2001/XMLSchema#anyURI",
                                        Value = "urn:be:e-contract:dss"
                                    },
                                    ResourceAttributeDesignator = new AttributeDesignatorType()
                                    {
                                        DataType = "http://www.w3.org/2001/XMLSchema#anyURI",
                                        AttributeId = "urn:oasis:names:tc:xacml:1.0:resource:resource-id"
                                    }
                                }
                            }
                        },
                        Actions = new ActionMatchType[][]
                        {
                            new ActionMatchType[]
                            {
                                new ActionMatchType() {
                                    MatchId = "urn:oasis:names:tc:xacml:1.0:function:string-equal",
                                    AttributeValue = new AttributeValueType()
                                    {
                                        DataType = "http://www.w3.org/2001/XMLSchema#string",
                                        Value = "sign"
                                    },
                                    ActionAttributeDesignator = new AttributeDesignatorType()
                                    {
                                        DataType = "http://www.w3.org/2001/XMLSchema#string",
                                        AttributeId = "urn:oasis:names:tc:xacml:1.0:action:action-id"
                                    }
                                }
                            }
                        }
                    },
                    Rule = rules
                };
            
            }
        }
    }
}
