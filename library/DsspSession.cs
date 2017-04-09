/*
 *  This file is part of DSS-P client.
 *  Copyright (C) 2014-2017 Egelke BVBA
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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace EContract.Dssp.Client
{
    /// <summary>
    /// Information linked to a single signature session.
    /// </summary>
    /// <remarks>
    /// A signature session consists of the following steps: upload document, start signature, end signature and download document.
    /// See the e-contract.be documentation for more information.
    /// </remarks>
    [Serializable]
    public class DsspSession
    {
        private static readonly TraceSource trace = new TraceSource("EContract.Dssp.Client");

        private static readonly TraceSource msgTrace = new TraceSource("EContract.Dssp.Client.MessageLogging");

        internal DsspSession()
        {
            ClientId = "msg-" + Guid.NewGuid().ToString();
            initSerializers();
        }

        [NonSerialized]
        private XmlSerializer requestSerializer;
        [NonSerialized]
        private XmlSerializer responseSerializer;
        [NonSerialized]
        private XmlSerializer tRefSerializer;

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            initSerializers();            
        }

        private void initSerializers()
        {
            requestSerializer = new XmlSerializer(typeof(PendingRequest), "urn:oasis:names:tc:dss:1.0:profiles:asynchronousprocessing:1.0");
            responseSerializer = new XmlSerializer(typeof(SignResponse), "urn:oasis:names:tc:dss:1.0:core:schema");
            tRefSerializer = new XmlSerializer(typeof(SecurityTokenReferenceType), null, new Type[0], new XmlRootAttribute("SecurityTokenReference"), "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd");
        }


        /// <summary>
        /// The session ID, client side
        /// </summary>
        /// <value>
        /// This corresponds to the MessageID of the BROWSER/POST protocol.
        /// </value>
        public String ClientId { get; set; }

        /// <summary>
        /// The session ID, server side
        /// </summary>
        /// <value>
        /// This corresponds to the ResponseID that is used with all steps.
        /// </value>
        public string ServerId { get; set; }

        /// <summary>
        /// The ID of the session key (aka token).
        /// </summary>
        public string KeyId { get; set; }

        /// <summary>
        /// The value of the session key.
        /// </summary>
        public byte[] KeyValue { get; set; }

        /// <summary>
        /// The Security token reference type
        /// </summary>
        public SecurityTokenReferenceType KeyReference { get; set; }

        /// <summary>
        /// Indicates how long the session should be kept.
        /// </summary>
        public DateTime ExpiresOn;

        /// <summary>
        /// Generates the html page that initiates the BROWSER/POST request for the current session.
        /// </summary>
        /// <remarks>
        /// The default language is used for the e-contract pages.
        /// </remarks>
        /// <param name="postAddress">The e-contract.be address, normally "https://www.e-contract.be/dss-ws/start"</param>
        /// <param name="landingUrl">Own url for the BROWSER/POST "SignResponse" response</param>
        /// <returns>The html page in the form of a string</returns>
        public string GeneratePendingRequestPage(string postAddress, string landingUrl)
        {
            return GeneratePendingRequestPage(new Uri(postAddress), new Uri(landingUrl));
        }

        /// <summary>
        /// Generates the html page that initiates the BROWSER/POST request for the current session.
        /// </summary>
        /// <remarks>
        /// The default language is used for the e-contract pages.
        /// </remarks>
        /// <param name="postAddress">The e-contract.be address, normally "https://www.e-contract.be/dss-ws/start"</param>
        /// <param name="landingUrl">Own url for the BROWSER/POST "SignResponse" response</param>
        /// <returns>The html page in the form of a string</returns>
        public string GeneratePendingRequestPage(Uri postAddress, Uri landingUrl)
        {
            return GeneratePendingRequestPage(postAddress, landingUrl, null);
        }

        /// <summary>
        /// Generates the html page that initiates the BROWSER/POST request for the current session.
        /// </summary>
        /// <param name="postAddress">The e-contract.be address, normally "https://www.e-contract.be/dss-ws/start"</param>
        /// <param name="landingUrl">Own url for the BROWSER/POST "SignResponse" response</param>
        /// <param name="language">The language of the e-contract.be pages, <c>null</c> for the default language</param>
        /// <returns>The html page in the form of a string</returns>
        public string GeneratePendingRequestPage(string postAddress, string landingUrl, string language)
        {
            return GeneratePendingRequestPage(new Uri(postAddress), new Uri(landingUrl), language);
        }

        /// <summary>
        /// Generates the html page that initiates the BROWSER/POST request for the current session.
        /// </summary>
        /// <param name="postAddress">The e-contract.be address, normally "https://www.e-contract.be/dss-ws/start"</param>
        /// <param name="landingUrl">Own url for the BROWSER/POST "SignResponse" response</param>
        /// <param name="language">The language of the e-contract.be pages, <c>null</c> for the default language</param>
        /// <returns>The html page in the form of a string</returns>
        public string GeneratePendingRequestPage(Uri postAddress, Uri landingUrl, string language)
        {
            return GeneratePendingRequestPage(postAddress, landingUrl, language, null, (Authorization) null);
        }

        /// <summary>
        /// Generates the html page that initiates the BROWSER/POST request for the current session.
        /// </summary>
        /// <param name="postAddress">The e-contract.be address, normally "https://www.e-contract.be/dss-ws/start"</param>
        /// <param name="landingUrl">Own url for the BROWSER/POST "SignResponse" response</param>
        /// <param name="language">The language of the e-contract.be pages, <c>null</c> for the default language</param>
        /// <param name="properties">Additional properties (location, role, visibility info, ...) for the signature request</param>
        /// <returns>The html page in the form of a string</returns>
        public string GeneratePendingRequestPage(Uri postAddress, Uri landingUrl, string language, SignatureRequestProperties properties)
        {
            return GeneratePendingRequestPage(postAddress, landingUrl, language, properties, (Authorization)null);
        }

        /// <summary>
        /// Generates the html page that initiates the BROWSER/POST request for the current session.
        /// </summary>
        /// <param name="postAddress">The e-contract.be address, normally "https://www.e-contract.be/dss-ws/start"</param>
        /// <param name="landingUrl">Own url for the BROWSER/POST "SignResponse" response</param>
        /// <param name="language">The language of the e-contract.be pages, <c>null</c> for the default language</param>
        /// <param name="authorization">The authorization that the signer must match too to be authorized</param>
        /// <returns>The html page in the form of a string</returns>
        public string GeneratePendingRequestPage(Uri postAddress, Uri landingUrl, string language, Authorization authorization)
        {
            return GeneratePendingRequestPage(postAddress, landingUrl, language, null, authorization);
        }

        /// <summary>
        /// Generates the html page that initiates the BROWSER/POST request for the current session.
        /// </summary>
        /// <param name="postAddress">The e-contract.be address, normally "https://www.e-contract.be/dss-ws/start"</param>
        /// <param name="landingUrl">Own url for the BROWSER/POST "SignResponse" response</param>
        /// <param name="language">The language of the e-contract.be pages, <c>null</c> for the default language</param>
        /// <param name="subjectRegex">Regular expression of the eID subject that the signer must match too to be authorized</param>
        /// <returns>The html page in the form of a string</returns>
        public string GeneratePendingRequestPage(Uri postAddress, Uri landingUrl, string language, string subjectRegex)
        {
            return GeneratePendingRequestPage(postAddress, landingUrl, language, null, subjectRegex);
        }

        /// <summary>
        /// Generates the html page that initiates the BROWSER/POST request for the current session.
        /// </summary>
        /// <param name="postAddress">The e-contract.be address, normally "https://www.e-contract.be/dss-ws/start"</param>
        /// <param name="landingUrl">Own url for the BROWSER/POST "SignResponse" response</param>
        /// <param name="language">The language of the e-contract.be pages, <c>null</c> for the default language</param>
        /// <param name="properties">Additional properties (location, role, visibility info, ...) for the signature request</param>
        /// <param name="subjectRegex">Regular expression of the eID subject that the signer must match too to be authorized</param>
        /// <returns>The html page in the form of a string</returns>
        public string GeneratePendingRequestPage(Uri postAddress, Uri landingUrl, string language, SignatureRequestProperties properties, string subjectRegex)
        {
            if (String.IsNullOrEmpty(subjectRegex))
                return GeneratePendingRequestPage(postAddress, landingUrl, language, properties, (Authorization)null);
            else
                return GeneratePendingRequestPage(postAddress, landingUrl, language, properties, Authorization.AllowDssSignIfMatchSubjectRegex(subjectRegex));
        }

        /// <summary>
        /// Generates the html page that initiates the BROWSER/POST request for the current session.
        /// </summary>
        /// <param name="postAddress">The e-contract.be address, normally "https://www.e-contract.be/dss-ws/start"</param>
        /// <param name="landingUrl">Own url for the BROWSER/POST "SignResponse" response</param>
        /// <param name="language">The language of the e-contract.be pages, <c>null</c> for the default language</param>
        /// <param name="properties">Additional properties (location, role, visibility info, ...) for the signature request</param>
        /// <param name="authorization">The authorization that the signer must match too to be authorized</param>
        /// <returns>The html page in the form of a string</returns>
        public string GeneratePendingRequestPage(Uri postAddress, Uri landingUrl, string language, SignatureRequestProperties properties, Authorization authorization)
        {
            var builder = new StringBuilder();

            builder.AppendLine("<html>");
            builder.AppendLine("<head><title>DSS-P Browser POST</title></head>");
            builder.AppendLine("<body>");
            builder.AppendLine("<p>Redirecting to the DSS-P Server...</p>");
            builder.AppendLine("<form name=\"dsspform\" method=\"post\" action=\"" + postAddress.ToString() + "\">");
            builder.Append("<input type=\"hidden\" name=\"PendingRequest\" value=\"");
            builder.Append(GeneratePendingRequest(landingUrl, language, properties, authorization));
            builder.AppendLine("\"/>");
            builder.AppendLine("</form>");
            builder.AppendLine("<script type=\"text/javascript\">");
            builder.AppendLine("window.onload = function() { document.forms[\"dsspform\"].submit(); };");
            builder.AppendLine("</script>");
            builder.AppendLine("</body>");
            builder.AppendLine("</html>");

            return builder.ToString();
        }

        /// <summary>
        /// Creates the pending request message for the current session.
        /// </summary>
        /// <remarks>
        /// The default language is used for the e-contract.be pages.
        /// </remarks>
        /// <param name="landingUrl">Own url for the BROWSER/POST "SignResponse" response</param>
        /// <returns>The base64 encoded PendingRequest, to be used as value for the "PendingRequest"-input</returns>
        public string GeneratePendingRequest(string landingUrl)
        {
            return GeneratePendingRequest(new Uri(landingUrl));
        }

        /// <summary>
        /// Creates the pending request message for the current session.
        /// </summary>
        /// <remarks>
        /// The default language is used for the e-contract.be pages.
        /// </remarks>
        /// <param name="landingUrl">Own url for the BROWSER/POST "SignResponse" response</param>
        /// <returns>The base64 encoded PendingRequest, to be used as value for the "PendingRequest"-input</returns>
        public string GeneratePendingRequest(Uri landingUrl)
        {
            return GeneratePendingRequest(landingUrl, null);
        }

        /// <summary>
        /// Creates a new pending request for the current session.
        /// </summary>
        /// <param name="landingUrl">The landing page of the SignResponse</param>
        /// <param name="language">The language of the e-contract.be pages, <c>null</c> for the default language</param>
        /// <returns>The base64 encoded PendingRequest, to be used as value for the "PendingRequest"-input</returns>
        public String GeneratePendingRequest(string landingUrl, string language)
        {
            return GeneratePendingRequest(new Uri(landingUrl), language);
        }

        /// <summary>
        /// Creates a new pending request for the current session.
        /// </summary>
        /// <param name="landingUrl">The landing page of the SignResponse</param>
        /// <param name="language">The language of the e-contract.be pages, <c>null</c> for the default language</param>
        /// <returns>The base64 encoded PendingRequest, to be used as value for the "PendingRequest"-input</returns>
        public string GeneratePendingRequest(Uri landingUrl, string language)
        {
            return GeneratePendingRequest(landingUrl, language, null, (Authorization) null);
        }

        /// <summary>
        /// Creates a new pending request for the current session.
        /// </summary>
        /// <param name="landingUrl">The landing page of the SignResponse</param>
        /// <param name="language">The language of the e-contract.be pages, <c>null</c> for the default language</param>
        /// <param name="properties">Additional properties (location, role, visibility info, ...) for the signature request</param>
        /// <returns>The base64 encoded PendingRequest, to be used as value for the "PendingRequest"-input</returns>
        public string GeneratePendingRequest(Uri landingUrl, string language, SignatureRequestProperties properties)
        {
            return GeneratePendingRequest(landingUrl, language, properties, (Authorization) null);
        }

        /// <summary>
        /// Creates a new pending request for the current session.
        /// </summary>
        /// <param name="landingUrl">The landing page of the SignResponse</param>
        /// <param name="language">The language of the e-contract.be pages, <c>null</c> for the default language</param>
        /// <param name="authorization">The authorization that the signer must match too to be authorized</param>
        /// <returns>The base64 encoded PendingRequest, to be used as value for the "PendingRequest"-input</returns>
        public string GeneratePendingRequest(Uri landingUrl, string language, Authorization authorization)
        {
            return GeneratePendingRequest(landingUrl, language, null, authorization);
        }

        /// <summary>
        /// Creates a new pending request for the current session.
        /// </summary>
        /// <param name="landingUrl">The landing page of the SignResponse</param>
        /// <param name="language">The language of the e-contract.be pages, <c>null</c> for the default language</param>
        /// <param name="subjectRegex">Regular expression of the eID subject that the signer must match too to be authorized</param>
        /// <returns>The base64 encoded PendingRequest, to be used as value for the "PendingRequest"-input</returns>
        public string GeneratePendingRequest(Uri landingUrl, string language, string subjectRegex)
        {
            return GeneratePendingRequest(landingUrl, language, null, subjectRegex);
        }

        /// <summary>
        /// Creates a new pending request for the current session.
        /// </summary>
        /// <param name="landingUrl">The landing page of the SignResponse</param>
        /// <param name="language">The language of the e-contract.be pages, <c>null</c> for the default language</param>
        /// <param name="properties">Additional properties (location, role, visibility info, ...) for the signature request</param>
        /// <param name="subjectRegex">Regular expression of the eID subject that the signer must match too to be authorized</param>
        /// <returns>The base64 encoded PendingRequest, to be used as value for the "PendingRequest"-input</returns>
        public string GeneratePendingRequest(Uri landingUrl, string language, SignatureRequestProperties properties, string subjectRegex)
        {
            if (string.IsNullOrEmpty(subjectRegex))
                return GeneratePendingRequest(landingUrl, language, properties, (Authorization)null);
            else
                return GeneratePendingRequest(landingUrl, language, properties, Authorization.AllowDssSignIfMatchSubjectRegex(subjectRegex));
        }

        /// <summary>
        /// Creates a new pending request for the current session.
        /// </summary>
        /// <param name="landingUrl">The landing page of the SignResponse</param>
        /// <param name="language">The language of the e-contract.be pages, <c>null</c> for the default language</param>
        /// <param name="properties">Additional properties (location, role, visibility info, ...) for the signature request</param>
        /// <param name="authorization">The optional authorization that the signer must match too to be authorized</param>
        /// <returns>The base64 encoded PendingRequest, to be used as value for the "PendingRequest"-input</returns>
        public string GeneratePendingRequest(Uri landingUrl, string language, SignatureRequestProperties properties, Authorization authorization)
        {
            if (landingUrl == null) throw new ArgumentNullException("landingUrl");

            //Prepare browser post message (to return)
            var pendingRequest = new PendingRequest()
            {
                OptionalInputs = new OptionalInputs()
                {
                    AdditionalProfile = "urn:oasis:names:tc:dss:1.0:profiles:asynchronousprocessing",
                    ResponseID = this.ServerId,
                    MessageID = new AttributedURIType()
                    {
                        Value = this.ClientId
                    },
                    Timestamp = new TimestampType()
                    {
                        Created = new AttributedDateTime()
                        {
                            Value = DateTime.UtcNow
                        },
                        Expires = new AttributedDateTime()
                        {
                            Value = DateTime.UtcNow.AddMinutes(10)
                        }
                    },
                    ReplyTo = new EndpointReferenceType()
                    {
                        Address = new AttributedURIType()
                        {
                            Value = landingUrl.AbsoluteUri
                        }
                    },
                    ReturnSignerIdentity = new ReturnSignerIdentity(),
                    Language = string.IsNullOrEmpty(language) ? null : language,
                    VisibleSignatureConfiguration = properties?.Configuration,
                    Policy = authorization?.Policy
                },
                
            };

            //Prepare Sign
            var pendingRequestXml = new XmlDocument()
            {
                PreserveWhitespace = true
            };
            using (var pendingRequestWriter = pendingRequestXml.CreateNavigator().AppendChild())
            {
                requestSerializer.Serialize(pendingRequestWriter, pendingRequest);
            }

            var signedXml = new SignedXml(pendingRequestXml);
            signedXml.SignedInfo.CanonicalizationMethod = SignedXml.XmlDsigExcC14NTransformUrl;
            signedXml.SignedInfo.SignatureMethod = SignedXml.XmlDsigHMACSHA1Url;
            var docRef = new Reference("")
            {
                DigestMethod = "http://www.w3.org/2000/09/xmldsig#sha1"
            };
            docRef.AddTransform(new XmlDsigEnvelopedSignatureTransform());
            docRef.AddTransform(new XmlDsigExcC14NTransform());
            signedXml.AddReference(docRef);

            //Add Key Info
            var keyRefXml = new XmlDocument() { PreserveWhitespace = true };
            if (null == tRefSerializer)
            {
                tRefSerializer = new XmlSerializer(typeof(SecurityTokenReferenceType), null, new Type[0], new XmlRootAttribute("SecurityTokenReference"), "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd");
            }
            using (var keyRefXmlWriter = keyRefXml.CreateNavigator().AppendChild())
            {
                tRefSerializer.Serialize(keyRefXmlWriter, this.KeyReference);
            }
            signedXml.KeyInfo = new KeyInfo();
            signedXml.KeyInfo.AddClause(new KeyInfoNode(keyRefXml.DocumentElement));

            //Compute signature
            signedXml.ComputeSignature(new HMACSHA1(this.KeyValue));

            //Append signature to document
            var nsmgr = new XmlNamespaceManager(pendingRequestXml.NameTable);
            nsmgr.AddNamespace("async", "urn:oasis:names:tc:dss:1.0:profiles:asynchronousprocessing:1.0");
            nsmgr.AddNamespace("dss", "urn:oasis:names:tc:dss:1.0:core:schema");
            pendingRequestXml.SelectSingleNode("/async:PendingRequest/dss:OptionalInputs", nsmgr).AppendChild(signedXml.GetXml());

            trace.TraceEvent(TraceEventType.Information, 0, "Generated pending request");
            msgTrace.TraceData(TraceEventType.Information, 0, pendingRequestXml.CreateNavigator());

            //Serialize and encode
            var stream = new MemoryStream();
            pendingRequestXml.Save(stream);
            
            return Convert.ToBase64String(stream.ToArray());
        }

        /// <summary>
        /// Validates the sign response from the BROWSER/POST response.
        /// </summary>
        /// <param name="signResponse">The received SignResponse, i.e. the value of the "SignResponse"-input</param>
        /// <exception cref="ArgumentNullException">When the parameters are null</exception>
        /// <exception cref="InvalidDataException">When the signResponse isn't the correct format or has invalid values</exception>
        /// <exception cref="RequestError">When the signResponse indicated there was an error with the request or with the user (e.g. if he cancels)</exception>
        /// <exception cref="InvalidOperationException">When the signRespoinse indicates an unknown error</exception>
        /// <returns>The signer of the document</returns>
        public NameIdentifierType ValidateSignResponse(string signResponse)
        {
            if (signResponse == null) throw new ArgumentNullException("signResponse");

            //Parse it.
            byte[] signResponseBytes;
            XmlDocument signResponseXml;
            SignResponse signResponseObject;
            try
            {
                signResponseBytes = Convert.FromBase64String(signResponse);
                signResponseXml = new XmlDocument() { PreserveWhitespace = true };
                signResponseXml.Load(new MemoryStream(signResponseBytes));
                trace.TraceEvent(TraceEventType.Information, 0, "Received sign response");
                msgTrace.TraceData(TraceEventType.Information, 0, signResponseXml.CreateNavigator());
                signResponseObject = (SignResponse)responseSerializer.Deserialize(new XmlNodeReader(signResponseXml));
            }
            catch (Exception e)
            {
                throw new InvalidDataException("The signResponse didn't have the right structure", e);
            }

            //Check the signature.
            var nsmgr = new XmlNamespaceManager(signResponseXml.NameTable);
            nsmgr.AddNamespace("ds", "http://www.w3.org/2000/09/xmldsig#");
            var xmlSignature = (XmlElement)signResponseXml.SelectSingleNode("//ds:Signature", nsmgr);

            var signedXml = new SignedXml(signResponseXml);
            signedXml.LoadXml(xmlSignature);
            if (!signedXml.CheckSignature(new HMACSHA1(this.KeyValue)))
            {
                throw new InvalidDataException("The signResponse has an invalid signature");
            }

            //Check the major result
            switch (signResponseObject.Result.ResultMajor)
            {
                case "urn:oasis:names:tc:dss:1.0:profiles:asynchronousprocessing:resultmajor:Pending":
                    break;
                case "urn:oasis:names:tc:dss:1.0:resultmajor:RequesterError":
                    switch (signResponseObject.Result.ResultMinor) {
                        case "urn:be:e-contract:dssp:1.0:resultminor:subject-not-authorized":
                            throw new AuthorizationError(
                                signResponseObject.Result.ResultMessage?.Value, 
                                signResponseObject.OptionalOutputs?.SignerIdentity);
                        default:
                            throw new RequestError(signResponseObject.Result.ResultMinor.Replace("urn:be:e-contract:dssp:1.0:resultminor:", ""));
                    }
                default:
                    throw new InvalidOperationException(signResponseObject.Result.ResultMajor);
            }

            //Check if the session and the response match
            if (this.ServerId != signResponseObject.OptionalOutputs.ResponseID)
                throw new InvalidDataException("The signResponse and session don't match (server id)");
            if (this.ClientId != signResponseObject.OptionalOutputs.RelatesTo.Value)
                throw new InvalidDataException("The signResponse and session don't match (client id)");

            //check the timestamp
            if (signResponseObject.OptionalOutputs.Timestamp.Created.Value > DateTime.UtcNow.AddMinutes(5))
                throw new InvalidDataException("The signResponse is not yet valid");
            if (signResponseObject.OptionalOutputs.Timestamp.Expires.Value < DateTime.UtcNow.AddMinutes(-5))
                throw new InvalidDataException("The signResponse is expired");

            //Return the signer
            return signResponseObject.OptionalOutputs.SignerIdentity;
        }
    }
}
