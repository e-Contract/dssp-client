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
using EContract.Dssp.Client.WcfBinding;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace EContract.Dssp.Client
{
    /// <summary>
    /// The DSS-P client for e-contract.
    /// </summary>
    /// <remarks>
    /// This is a wrapper class for the e-contract service.  It does not contain direct support of the BROWSER/POST protocol, but provides the nesesary input
    /// and processes its output.
    /// </remarks>
    public class DsspClient
    {

        private readonly Random rand = new Random();
        private XmlSerializer requestSerializer = new XmlSerializer(typeof(PendingRequest), "urn:oasis:names:tc:dss:1.0:profiles:asynchronousprocessing:1.0");
        private XmlSerializer responseSerializer = new XmlSerializer(typeof(SignResponse), "urn:oasis:names:tc:dss:1.0:core:schema");
        private XmlSerializer tRefSerializer = new XmlSerializer(typeof(SecurityTokenReferenceType), null, new Type[0], new XmlRootAttribute("SecurityTokenReference"), "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd");

        /// <summary>
        /// The e-contract signature type.
        /// </summary>
        /// <value>
        /// Can be empty (the default) at which e-contract will select the most appropriate signature type.
        /// </value>
        public string SignatureType { get; set; }

        /// <summary>
        /// The address of e-contract DSS-P service.
        /// </summary>
        public EndpointAddress Address { get; set; }

        /// <summary>
        /// Basic constructor.
        /// </summary>
        /// <remarks>
        /// Client with default signature type for the specified address.
        /// </remarks>
        /// <param name="address">The address of the e-contract DSS-P service</param>
        public DsspClient(EndpointAddress address)
            : this(address, null)
        {

        }

        /// <summary>
        /// Full Constructor
        /// </summary>
        /// <param name="address">The address of the e-contract DSS-P service</param>
        /// <param name="signatureType">The signature type that is required, see e-contract documentation</param>
        public DsspClient(EndpointAddress address, string signatureType)
        {
            this.Address = address;
            this.SignatureType = SignatureType;
        }

        /// <summary>
        /// Uploads a document to e-Contract.
        /// </summary>
        /// <remarks>
        /// Uploads a document to e-Contract and returns the session for future references.
        /// </remarks>
        /// <param name="document">The document to be signed</param>
        /// <returns>The session, required for the BROWSER/POST protocol and the donwload of the signed message</returns>
        public async Task<DsspSession> UploadDocumentAsync(Document document)
        {
            if (document == null) throw new ArgumentNullException("document");

            //New session & client
            var session = DsspSession.NewSession();
            var documentId = "doc-" + Guid.NewGuid().ToString();
            var client = new DigitalSignatureServicePortTypeClient(new PlainDsspBinding(), Address);

            //Prepare
            byte[] clientNonce = new byte[32];
            rand.NextBytes(clientNonce);
            Psha1DerivedKeyGenerator pSha1 = new Psha1DerivedKeyGenerator(clientNonce);

            //Make request
            SignRequest request = new SignRequest();
            request.Profile = "urn:be:e-contract:dssp:1.0";
            request.OptionalInputs = new OptionalInputs();
            request.OptionalInputs.AdditionalProfile = "urn:oasis:names:tc:dss:1.0:profiles:asynchronousprocessing";
            request.OptionalInputs.RequestSecurityToken = new RequestSecurityTokenType();
            request.OptionalInputs.RequestSecurityToken.TokenType = "http://docs.oasis-open.org/ws-sx/ws-secureconversation/200512/sct";
            request.OptionalInputs.RequestSecurityToken.RequestType = "http://docs.oasis-open.org/ws-sx/ws-trust/200512/Issue";
            request.OptionalInputs.RequestSecurityToken.Entropy = new EntropyType();
            request.OptionalInputs.RequestSecurityToken.Entropy.BinarySecret = new BinarySecretType();
            request.OptionalInputs.RequestSecurityToken.Entropy.BinarySecret.Type = "http://docs.oasis-open.org/ws-sx/ws-trust/200512/Nonce";
            request.OptionalInputs.RequestSecurityToken.Entropy.BinarySecret.Value = clientNonce;

            request.OptionalInputs.SignatureType = SignatureType;
            request.OptionalInputs.SignaturePlacement = new SignaturePlacement();
            request.OptionalInputs.SignaturePlacement.CreateEnvelopedSignature = true;
            request.OptionalInputs.SignaturePlacement.CreateEnvelopedSignatureSpecified = true;
            request.OptionalInputs.SignaturePlacement.WhichDocument = documentId;

            request.InputDocuments = new InputDocuments();
            request.InputDocuments.Document = new DocumentType[1];
            request.InputDocuments.Document[0] = new DocumentType();
            request.InputDocuments.Document[0].ID = documentId;
            request.InputDocuments.Document[0].Base64Data = new Base64Data();
            request.InputDocuments.Document[0].Base64Data.MimeType = document.MimeType;

            var memStream = document.Content as MemoryStream;
            if (memStream == null)
            {
                memStream = new MemoryStream();
                await document.Content.CopyToAsync(memStream);
            }
            request.InputDocuments.Document[0].Base64Data.Value = memStream.ToArray();

            //Send
            signResponse1 responseWrapper = await client.signAsync(request);
            SignResponse response = responseWrapper.SignResponse;
            
            //Check response
            switch(response.Result.ResultMajor) 
            {
                case "urn:oasis:names:tc:dss:1.0:profiles:asynchronousprocessing:resultmajor:Pending":
                    break;
                case "urn:oasis:names:tc:dss:1.0:resultmajor:RequesterError":
                    throw new RequestError(response.Result.ResultMinor.Replace("urn:be:e-contract:dssp:1.0:resultminor:", ""));
                default:
                    throw new InvalidOperationException(response.Result.ResultMajor);
            }

            //Capture session info & store it
            session.ServerId = response.OptionalOutputs.ResponseID;
            var securityTokenResponse = response.OptionalOutputs.RequestSecurityTokenResponseCollection.RequestSecurityTokenResponse[0];
            session.KeyId = securityTokenResponse.RequestedSecurityToken.SecurityContextToken.Identifier;
            session.KeyValue = pSha1.GenerateDerivedKey(securityTokenResponse.Entropy.BinarySecret.Value, (int)securityTokenResponse.KeySize);
            session.KeyReference = securityTokenResponse.RequestedUnattachedReference.SecurityTokenReference;
            session.ExpiresOn = securityTokenResponse.Lifetime.Expires.Value;

            return session;
        }

        /// <summary>
        /// Downloads the document that was uploaded before and signed via the BROWSER/POST protocol.
        /// </summary>
        /// <remarks>
        /// The session is closed when the downloads finishes, it can't be reused afterward and should be removed from the storage.
        /// </remarks>
        /// <param name="session">The session linked to the uploaded document</param>
        /// <returns>The document with signature, including id and mimeType</returns>
        /// <exception cref="ArgumentException">When the signResponse isn't valid, including its signature</exception>
        /// <exception cref="InvalidOperationException">When the e-contract service returns an error</exception>
        public async Task<Document> DownloadDocumentAsync(DsspSession session)
        {
            if (session == null) throw new ArgumentNullException("session");

            //Download the signed document
            var client = new DigitalSignatureServicePortTypeClient(new ScDsspBinding(), Address);
            client.ChannelFactory.Endpoint.Behaviors.Remove<ClientCredentials>();
            client.ChannelFactory.Endpoint.Behaviors.Add(new ScDsspClientCredentials(session.KeyId, session.KeyValue));

            var downloadRequest = new PendingRequest();
            downloadRequest.OptionalInputs = new OptionalInputs();
            downloadRequest.OptionalInputs.AdditionalProfile = "urn:oasis:names:tc:dss:1.0:profiles:asynchronousprocessing";
            downloadRequest.OptionalInputs.ResponseID = session.ServerId;

            downloadRequest.OptionalInputs.RequestSecurityToken = new RequestSecurityTokenType();
            downloadRequest.OptionalInputs.RequestSecurityToken.RequestType = "http://docs.oasis-open.org/ws-sx/ws-trust/200512/Cancel";
            downloadRequest.OptionalInputs.RequestSecurityToken.CancelTarget = new CancelTargetType();
            downloadRequest.OptionalInputs.RequestSecurityToken.CancelTarget.SecurityTokenReference = new SecurityTokenReferenceType();
            downloadRequest.OptionalInputs.RequestSecurityToken.CancelTarget.SecurityTokenReference.Reference = new ReferenceType();
            downloadRequest.OptionalInputs.RequestSecurityToken.CancelTarget.SecurityTokenReference.Reference.ValueType = "http://docs.oasis-open.org/ws-sx/ws-secureconversation/200512/sct";
            downloadRequest.OptionalInputs.RequestSecurityToken.CancelTarget.SecurityTokenReference.Reference.URI =  session.KeyId;

            var downloadResponse = await client.pendingRequestAsync(downloadRequest);

            //check the download reponse
            switch (downloadResponse.SignResponse.Result.ResultMajor)
            {
                case "urn:oasis:names:tc:dss:1.0:resultmajor:Success":
                    break;
                default:
                    throw new InvalidOperationException(downloadResponse.SignResponse.Result.ResultMajor);
            }

            //Return the downloaded document (we assume there is only a single document)
            var doc = new Document(downloadResponse.SignResponse.OptionalOutputs.DocumentWithSignature.Document);

            return doc;
        }

        /// <summary>
        /// Validates the provided document via the e-contract service.
        /// </summary>
        /// <param name="document">The document that contains a signature</param>
        /// <returns>The security information of the document, containting information like the signer</returns>
        /// <exception cref="ArgumentNullException">When there is no document provided</exception>
        /// <exception cref="IncorrectSignatureException">When the provided document has an invalid signature</exception>
        /// <exception cref="RequestError">When the request was invalid, e.g. unsupported mime type</exception>
        /// <exception cref="InvalidOperationException">All other errors indicated by the service</exception>
        public async Task<SecurityInfo> VerifyAsync(Document document)
        {
            if (document == null) throw new ArgumentNullException("document");

            var client = new DigitalSignatureServicePortTypeClient(new PlainDsspBinding(), Address);

            var request = new VerifyRequest();
            request.Profile = "urn:be:e-contract:dssp:1.0";

            request.OptionalInputs = new OptionalInputs();
            request.OptionalInputs.ReturnVerificationReport = new ReturnVerificationReport();
            request.OptionalInputs.ReturnVerificationReport.IncludeVerifier = true;
            request.OptionalInputs.ReturnVerificationReport.IncludeCertificateValues = true;

            request.InputDocuments = new InputDocuments();
            request.InputDocuments.Document = new DocumentType[1];
            request.InputDocuments.Document[0] = new DocumentType();
            request.InputDocuments.Document[0].ID = "doc-" + Guid.NewGuid().ToString();
            request.InputDocuments.Document[0].Base64Data = new Base64Data();
            request.InputDocuments.Document[0].Base64Data.MimeType = document.MimeType;

            var memStream = document.Content as MemoryStream;
            if (memStream == null)
            {
                memStream = new MemoryStream();
                await document.Content.CopyToAsync(memStream);
            }
            request.InputDocuments.Document[0].Base64Data.Value = memStream.ToArray();

            var response = await client.verifyAsync(request);

            //Check response
            switch (response.VerifyResponse1.Result.ResultMajor)
            {
                case "urn:oasis:names:tc:dss:1.0:resultmajor:Success":
                    break;
                case "urn:oasis:names:tc:dss:1.0:resultmajor:RequesterError":
                    if (response.VerifyResponse1.Result.ResultMinor == "urn:oasis:names:tc:dss:1.0:resultminor:invalid:IncorrectSignature")
                    {
                        throw new IncorrectSignatureException(response.VerifyResponse1.Result.ResultMessage.Value);
                    } else {
                        throw new RequestError(response.VerifyResponse1.Result.ResultMinor.Replace("urn:be:e-contract:dssp:1.0:resultminor:", ""));
                    }
                default:
                    throw new InvalidOperationException(response.VerifyResponse1.Result.ResultMajor);
            }

            SecurityInfo result = new SecurityInfo();
            result.TimeStampValidity = response.VerifyResponse1.OptionalOutputs.TimeStampRenewal.Before;
            result.Signatures = new List<SignatureInfo>();
            foreach(var report in response.VerifyResponse1.OptionalOutputs.VerificationReport.IndividualReport)
            { 
                //double check
                if (report.Result.ResultMajor != "urn:oasis:names:tc:dss:1.0:resultmajor:Success") throw new InvalidOperationException(report.Result.ResultMajor);

                var info = new SignatureInfo();
                info.SigningTime = DateTime.Parse(report.SignedObjectIdentifier.SignedProperties.SignedSignatureProperties.SigningTime, CultureInfo.InvariantCulture);
                info.Signer = new X509Certificate2(report.Details.DetailedSignatureReport.CertificatePathValidity.PathValidityDetail.CertificateValidity[0].CertificateValue);
                result.Signatures.Add(info);
            }
            return result;
        }
    }
}
