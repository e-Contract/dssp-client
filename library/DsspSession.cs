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

using EContract.Dssp.Client.Proxy;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace EContract.Dssp.Client
{
    [Serializable]
    public class DsspSession
    {
        private XmlSerializer requestSerializer = new XmlSerializer(typeof(PendingRequest), "urn:oasis:names:tc:dss:1.0:profiles:asynchronousprocessing:1.0");
        private XmlSerializer responseSerializer = new XmlSerializer(typeof(SignResponse), "urn:oasis:names:tc:dss:1.0:core:schema");
        private XmlSerializer tRefSerializer = new XmlSerializer(typeof(SecurityTokenReferenceType), null, new Type[0], new XmlRootAttribute("SecurityTokenReference"), "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd");

        public DsspSession()
        {

        }

        internal DsspSession(string documentId)
        {
            this.DocumentId = documentId;
            this.ClientId = "msg-" + Guid.NewGuid().ToString();
        }

        public DsspSession(string documentId, string clientId, string serverId, string keyId, byte[] keyValue, SecurityTokenReferenceType keyReference)
        {
            this.DocumentId = documentId;
            this.ClientId = clientId;
            this.ServerId = serverId;
            this.KeyId = keyId;
            this.KeyValue = keyValue;
            this.KeyReference = keyReference;
        }

        /// <summary>
        /// The ID of the document that was signed.
        /// </summary>
        /// <remarks>
        /// This is either the document ID you provided or the random r
        /// </remarks>
        public String DocumentId { get; set; }

        /// <summary>
        /// The session ID, client side
        /// </summary>
        public String ClientId { get; set; }

        /// <summary>
        /// The session ID, server side
        /// </summary>
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

        public string GeneratePendingRequestPage(Uri postAddress, Uri landingUrl)
        {
            return GeneratePendingRequestPage(postAddress, landingUrl, null);
        }

        public string GeneratePendingRequestPage(Uri postAddress, Uri landingUrl, string language)
        {
            var builder = new StringBuilder();

            builder.AppendLine("<html>");
            builder.AppendLine("<head><title>DSS-P Browser POST</title></head>");
            builder.AppendLine("<body>");
            builder.AppendLine("<p>Redirecting to the DSS-P Server...</p>");
            builder.AppendLine("<form name=\"dsspform\" method=\"post\" action=\"" + postAddress.ToString() + "\">");
            builder.Append("<input type=\"hidden\" name=\"PendingRequest\" value=\"");
            builder.Append(GeneratePendingRequest(landingUrl, language));
            builder.AppendLine("\"/>");
            builder.AppendLine("</form>");
            builder.AppendLine("<script type=\"text/javascript\">");
            builder.AppendLine("window.onload = function() { document.forms[\"dsspform\"].submit(); };");
            builder.AppendLine("</script>");
            builder.AppendLine("</body>");
            builder.AppendLine("</html>");

            return builder.ToString();
        }

        public string GeneratePendingRequest(Uri landingUrl)
        {
            return GeneratePendingRequest(landingUrl, null);
        }

        /// <summary>
        /// Creates a new pending request for the provided session.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="landingUrl">The landing page of the SignResponse</param>
        /// <param name="language">The language of the e-contract pages, <c>null</c> for default</param>
        /// <returns>The base64 encoded "PendingRequest" value for the BROWSER/POST</returns>
        public string GeneratePendingRequest(Uri landingUrl, string language)
        {
            //Prepare browser post message (to return)
            PendingRequest pendingRequest = new PendingRequest();
            pendingRequest.OptionalInputs = new OptionalInputs();
            pendingRequest.OptionalInputs.AdditionalProfile = "urn:oasis:names:tc:dss:1.0:profiles:asynchronousprocessing";
            pendingRequest.OptionalInputs.ResponseID = this.ServerId;
            pendingRequest.OptionalInputs.MessageID = new AttributedURIType();
            pendingRequest.OptionalInputs.MessageID.Value = this.ClientId;
            pendingRequest.OptionalInputs.Timestamp = new TimestampType();
            pendingRequest.OptionalInputs.Timestamp.Created = new AttributedDateTime();
            pendingRequest.OptionalInputs.Timestamp.Created.Value = DateTime.UtcNow;
            pendingRequest.OptionalInputs.Timestamp.Expires = new AttributedDateTime();
            pendingRequest.OptionalInputs.Timestamp.Expires.Value = DateTime.UtcNow.AddMinutes(10);
            pendingRequest.OptionalInputs.ReplyTo = new EndpointReferenceType();
            pendingRequest.OptionalInputs.ReplyTo.Address = new AttributedURIType();
            pendingRequest.OptionalInputs.ReplyTo.Address.Value = landingUrl.AbsoluteUri;
            pendingRequest.OptionalInputs.ReturnSignerIdentity = new ReturnSignerIdentity();
            pendingRequest.OptionalInputs.Language = language;

            //Prepare Sign
            var pendingRequestXml = new XmlDocument();
            pendingRequestXml.PreserveWhitespace = true;
            using (var pendingRequestWriter = pendingRequestXml.CreateNavigator().AppendChild())
            {
                requestSerializer.Serialize(pendingRequestWriter, pendingRequest);
            }

            SignedXml signedXml = new SignedXml(pendingRequestXml);
            signedXml.SignedInfo.CanonicalizationMethod = SignedXml.XmlDsigExcC14NTransformUrl;
            signedXml.SignedInfo.SignatureMethod = SignedXml.XmlDsigHMACSHA1Url;
            var docRef = new Reference("");
            docRef.DigestMethod = "http://www.w3.org/2000/09/xmldsig#sha1";
            docRef.AddTransform(new XmlDsigEnvelopedSignatureTransform());
            docRef.AddTransform(new XmlDsigExcC14NTransform());
            signedXml.AddReference(docRef);

            //Add Key Info
            var keyRefXml = new XmlDocument();
            keyRefXml.PreserveWhitespace = true;
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

            //Serialize and encode
            var stream = new MemoryStream();
            pendingRequestXml.Save(stream);
            return Convert.ToBase64String(stream.ToArray());
        }

        /// <summary>
        /// Validates the sign response and returns the signer if present.
        /// </summary>
        /// <param name="signResponse">The received SignResponse</param>
        /// <exception cref="ArgumentNullException">When the signResponse is null</exception>
        /// <exception cref="InvalidDataException">When the signResponse isn't the correct format or has invalid values</exception>
        /// <exception cref="RequestError">When the signResponse indicated there was an error with the request or with the user</exception>
        /// <exception cref="InvalidOperationException">When the signRespoinse indicates an unkown error</exception>
        /// <returns>The signer of the document (currently not returned by the service)</returns>
        public NameIdentifierType ValidateSignResponse(string signResponse)
        {
            if (signResponse == null) throw new ArgumentNullException("signResponse");

            //Parse it.
            byte[] signResponseBytes;
            SignResponse signResponseObject;
            try
            {
                signResponseBytes = Convert.FromBase64String(signResponse);
                signResponseObject = (SignResponse)responseSerializer.Deserialize(new MemoryStream(signResponseBytes));
            }
            catch (Exception e)
            {
                throw new InvalidDataException("The signResponse didn't have the right structure", e);
            }

            //Check the signature.
            XmlDocument xml = new XmlDocument();
            xml.PreserveWhitespace = true;
            xml.Load(new MemoryStream(signResponseBytes));

            var nsmgr = new XmlNamespaceManager(xml.NameTable);
            nsmgr.AddNamespace("ds", "http://www.w3.org/2000/09/xmldsig#");
            var xmlSignature = (XmlElement)xml.SelectSingleNode("//ds:Signature", nsmgr);

            SignedXml signedXml = new SignedXml(xml);
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
                    throw new RequestError(signResponseObject.Result.ResultMinor.Replace("urn:be:e-contract:dssp:1.0:resultminor:", ""));
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
