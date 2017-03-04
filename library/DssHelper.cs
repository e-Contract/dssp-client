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

using EContract.Dssp.Client.Proxy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EContract.Dssp.Client
{
    internal static class DssHelper
    {

        public static SubjectMatchType ToSubjectX500NameMatch(this String name)
        {
            return new SubjectMatchType()
            {
                MatchId = "urn:oasis:names:tc:xacml:1.0:function:x500Name-equal",
                AttributeValue = new AttributeValueType()
                {
                    DataType = "urn:oasis:names:tc:xacml:1.0:data-type:x500Name",
                    Value = name
                },
                SubjectAttributeDesignator = new SubjectAttributeDesignatorType()
                {
                    DataType = "urn:oasis:names:tc:xacml:1.0:data-type:x500Name",
                    AttributeId = "urn:oasis:names:tc:xacml:1.0:subject:subject-id"
                }
            };
        }

        public static SubjectMatchType ToSubjectX500NameRegExMatch(this String regEx)
        {
            return new SubjectMatchType()
            {
                MatchId = "urn:oasis:names:tc:xacml:2.0:function:x500Name-regexp-match",
                AttributeValue = new AttributeValueType()
                {
                    DataType = "http://www.w3.org/2001/XMLSchema#string",
                    Value = regEx
                },
                SubjectAttributeDesignator = new SubjectAttributeDesignatorType()
                {
                    DataType = "urn:oasis:names:tc:xacml:1.0:data-type:x500Name",
                    AttributeId = "urn:oasis:names:tc:xacml:1.0:subject:subject-id"
                }
            };
        }

        public static SubjectMatchType ToEidCardNrMatch(this String nr)
        {
            return new SubjectMatchType()
            {
                MatchId = "urn:oasis:names:tc:xacml:1.0:function:string-equal",
                AttributeValue = new AttributeValueType()
                {
                    DataType = "http://www.w3.org/2001/XMLSchema#string",
                    Value = nr
                },
                SubjectAttributeDesignator = new SubjectAttributeDesignatorType()
                {
                    DataType = "http://www.w3.org/2001/XMLSchema#string",
                    AttributeId = "urn:be:e-contract:dss:eid:card-number"
                }
            };
        }

        public static RuleType ToSubjectRule(this SubjectMatchType[] subjects, EffectType effect)
        {
            if (subjects == null || subjects.Length == 0) return null;

            return new RuleType()
            {
                RuleId = "rule-" + Guid.NewGuid().ToString(),
                Effect = effect,
                Target = new TargetType()
                {
                    Subjects = subjects.Select(x => new SubjectMatchType[] { x }).ToArray()
                }
            };
        }
    }
}
