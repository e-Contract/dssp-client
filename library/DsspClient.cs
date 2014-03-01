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
        private TraceSource msgTrace = new TraceSource("EContract.Dssp.Client.MessageLogging");

        private readonly Random rand = new Random();
        private XmlSerializer requestSerializer = new XmlSerializer(typeof(PendingRequest), "urn:oasis:names:tc:dss:1.0:profiles:asynchronousprocessing:1.0");
        private XmlSerializer responseSerializer = new XmlSerializer(typeof(SignResponse), "urn:oasis:names:tc:dss:1.0:core:schema");
        private XmlSerializer tRefSerializer = new XmlSerializer(typeof(SecurityTokenReferenceType), null, new Type[0], new XmlRootAttribute("SecurityTokenReference"), "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd");

        /// <summary>
        /// The own URL at which e-contract must post the SignResponse.
        /// </summary>
        /// <value>
        /// The default is empty, must be provide in in order to use the <see cref="UploadDocumentAsync()"/> method.
        /// </value>
        public Uri LandingPage { get; set; }

        /// <summary>
        /// The e-contract signature type.
        /// </summary>
        /// <value>
        /// Can be empty (the default) at which e-contract will select the most appropriate signature type.
        /// </value>
        public string SignatureType { get; set; }


        /// <summary>
        /// The total time the signature process may take, including the user interaction.
        /// </summary>
        /// <value>
        /// Default to 30 minutes.
        /// </value>
        public TimeSpan TransactionTimeout { get; set; }

        /// <summary>
        /// The place where session should be kept.
        /// </summary>
        /// <remarks>
        /// The library comes with 2 implementation <see cref="DsspSessionHttpSessionStore"/> and <see cref=" DsspSessionMemoryStore"/>.
        /// The former is prefered due the additional security it provides, but can't be used in controllers.
        /// </remarks>
        public IDsspSessionStore SessionStore { get; set; }

        /// <summary>
        /// The address of e-contract DSS-P service.
        /// </summary>
        public EndpointAddress Address { get; set; }

        /// <summary>
        /// Client for the DSS-P service of e-contract.
        /// </summary>
        /// <param name="landingPage">The url of the own application that will recieve the responses.</param>
        /// <param name="address">The address of the e-contract DSS-P service</param>
        /// <param name="store">Implementation where to store the session, e.g. <see cref="DsspSessionHttpSessionStore"/></param>
        public DsspClient(Uri landingPage, EndpointAddress address, IDsspSessionStore store)
        {
            this.LandingPage = landingPage;
            this.Address = address;
            this.SessionStore = store;

            this.TransactionTimeout = new TimeSpan(0, 30, 0);
        }

        /// <summary>
        /// Uploads a document to be signed via the browser.
        /// </summary>
        /// <remarks>
        /// This method uploads the document to e-contract and returns the "PendingRequest" value for the
        /// BROWSER/POST.
        /// </remarks>
        /// <param name="document">The document to be signed</param>
        /// <returns>The content for the "PendingRequest"-input that must be send to e-contract via BROWSER/POST</returns>
        public async Task<byte[]> UploadDocumentAsync(Document document)
        {
            //TODO: Asserts!

            //New session & client
            var session = DsspSession.New(document.Id);
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
            request.OptionalInputs.SignaturePlacement.WhichDocument = document.Id;

            request.InputDocuments = new InputDocuments();
            request.InputDocuments.Document = new DocumentType[1];
            request.InputDocuments.Document[0] = new DocumentType();
            request.InputDocuments.Document[0].ID = document.Id;
            request.InputDocuments.Document[0].Base64Data = new Base64Data();
            request.InputDocuments.Document[0].Base64Data.MimeType = document.MimeType;

            MemoryStream memStream = new MemoryStream();
            await document.Content.CopyToAsync(memStream); //TODO: maybe put in advance so we can wait later
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
            SessionStore.Store(session);

            //Prepare browser post message (to return)
            PendingRequest pendingRequest = new PendingRequest();
            pendingRequest.OptionalInputs = new OptionalInputs();
            pendingRequest.OptionalInputs.AdditionalProfile = "urn:oasis:names:tc:dss:1.0:profiles:asynchronousprocessing";
            pendingRequest.OptionalInputs.ResponseID = session.ServerId;
            pendingRequest.OptionalInputs.MessageID = new AttributedURIType();
            pendingRequest.OptionalInputs.MessageID.Value = session.ClientId;
            pendingRequest.OptionalInputs.Timestamp = new TimestampType();
            pendingRequest.OptionalInputs.Timestamp.Created = new AttributedDateTime();
            pendingRequest.OptionalInputs.Timestamp.Created.Value = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssK");
            pendingRequest.OptionalInputs.Timestamp.Expires = new AttributedDateTime();
            pendingRequest.OptionalInputs.Timestamp.Expires.Value = DateTime.UtcNow.AddMinutes(10).ToString("yyyy-MM-ddTHH:mm:ssK");
            pendingRequest.OptionalInputs.ReplyTo = new EndpointReferenceType();
            pendingRequest.OptionalInputs.ReplyTo.Address = new AttributedURIType();
            pendingRequest.OptionalInputs.ReplyTo.Address.Value = LandingPage.AbsoluteUri;

            pendingRequest.OptionalInputs.ReturnSignerIdentity = new ReturnSignerIdentity();

            if (document.Language != null) pendingRequest.OptionalInputs.Language = document.Language;

            //Sign
            MemoryStream stream = new MemoryStream();
            requestSerializer.Serialize(stream, pendingRequest);
            stream.Position = 0;

            XmlDocument xml = new XmlDocument();
            xml.PreserveWhitespace = true;
            xml.Load(stream);

            SignedXml signedXml = new SignedXml(xml);
            signedXml.SignedInfo.CanonicalizationMethod = SignedXml.XmlDsigExcC14NTransformUrl;
            signedXml.SignedInfo.SignatureMethod = SignedXml.XmlDsigHMACSHA1Url; 
            var docRef = new Reference("");
            docRef.DigestMethod = "http://www.w3.org/2000/09/xmldsig#sha1";
            docRef.AddTransform(new XmlDsigEnvelopedSignatureTransform());
            docRef.AddTransform(new XmlDsigExcC14NTransform());
            signedXml.AddReference(docRef);

            stream = new MemoryStream();
            tRefSerializer.Serialize(stream, securityTokenResponse.RequestedUnattachedReference.SecurityTokenReference);
            stream.Position = 0;
            XmlDocument keyInfoXml = new XmlDocument();
            keyInfoXml.Load(stream);

            signedXml.KeyInfo = new KeyInfo();
            signedXml.KeyInfo.AddClause(new KeyInfoNode(keyInfoXml.DocumentElement));
            
            signedXml.ComputeSignature(new HMACSHA1(session.KeyValue));

            stream = new MemoryStream();
            var nsmgr = new XmlNamespaceManager(xml.NameTable);
            nsmgr.AddNamespace("async", "urn:oasis:names:tc:dss:1.0:profiles:asynchronousprocessing:1.0");
            nsmgr.AddNamespace("dss", "urn:oasis:names:tc:dss:1.0:core:schema");
            xml.SelectSingleNode("/async:PendingRequest/dss:OptionalInputs", nsmgr).AppendChild(signedXml.GetXml());
            xml.Save(stream);

            //msgTrace.TraceData(TraceEventType.Information, 1, Encoding.UTF8.GetString(stream.ToArray()));

            return stream.ToArray();
        }

        /// <summary>
        /// Downloads the document that was signed via the browser.
        /// </summary>
        /// <remarks>
        /// This method gets the document from e-contract that corresponds to the provided "SignResponse" value which was
        /// received via BROWSER/POST.  The SignedDocument already forsees the signer information, which is currently
        /// foreseen by e-contact but not yet implemented.
        /// </remarks>
        /// <param name="signResponse">The "SignResponse"-input that was send by e-contract via BROWSER/POST</param>
        /// <returns>The signed document, with signer information (currently not provided)</returns>
        /// <exception cref="ArgumentException">When the signResponse isn't valid, including its signature</exception>
        public async Task<SignedDocument> DownloadDocumentAsync(byte[] signResponse)
        {
            //msgTrace.TraceData(TraceEventType.Information, 2, Encoding.UTF8.GetString(signResponse));

            //Parse it.
            SignResponse response;
            try
            {
                response = (SignResponse)responseSerializer.Deserialize(new MemoryStream(signResponse));
            }
            catch (Exception e)
            {
                throw new ArgumentException("The signResponse didn't have the right structure", "signResponse", e);
            }

            //lets find the session
            DsspSession session = SessionStore.Load(response.OptionalOutputs.RelatesTo.Value);

            //Before we continue, lest make sure the receive message is valid.
            XmlDocument xml = new XmlDocument();
            xml.PreserveWhitespace = true;
            xml.Load(new MemoryStream(signResponse));

            var nsmgr = new XmlNamespaceManager(xml.NameTable);
            nsmgr.AddNamespace("ds", "http://www.w3.org/2000/09/xmldsig#");
            var xmlSignature = (XmlElement) xml.SelectSingleNode("//ds:Signature", nsmgr);

            SignedXml signedXml = new SignedXml(xml);
            signedXml.LoadXml(xmlSignature);
            if (!signedXml.CheckSignature(new HMACSHA1(session.KeyValue)))
            {
                throw new ArgumentException("The signResponse has an invalid signature", "signResponse");
            }

            //Check the result
            switch (response.Result.ResultMajor)
            {
                case "urn:oasis:names:tc:dss:1.0:profiles:asynchronousprocessing:resultmajor:Pending":
                    break;
                case "urn:oasis:names:tc:dss:1.0:resultmajor:RequesterError":
                    throw new RequestError(response.Result.ResultMinor.Replace("urn:be:e-contract:dssp:1.0:resultminor:", ""));
                default:
                    throw new InvalidOperationException(response.Result.ResultMajor);
            }
            //TODO, check timestamp...

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
                    throw new InvalidOperationException(response.Result.ResultMajor);
            }

            //Remove the session from the store
            SessionStore.Remove(session.Id);

            //Return the downloaded document (we assume there is only a single document)
            return new SignedDocument(response.OptionalOutputs.SignerIdentity,
                downloadResponse.SignResponse.OptionalOutputs.DocumentWithSignature.Document);
        }

        /// <summary>
        /// Validates the provided document via the e-contract service.
        /// </summary>
        /// <param name="document">The document that contains a signature</param>
        /// <returns>The security information of the document, containting signature information</returns>
        /// <exception cref="RequestError">When the request was invalid, e.g. unsupported mime type</exception>
        public async Task<SecurityInfo> VerifyAsync(Document document)
        {
            var client = new DigitalSignatureServicePortTypeClient(new PlainDsspBinding(), Address);

            var request = new VerifyRequest();
            request.Profile = "urn:be:e-contract:dssp:1.0";

            MemoryStream memStream = new MemoryStream();
            Task memCopy = document.Content.CopyToAsync(memStream);

            request.OptionalInputs = new OptionalInputs();
            request.OptionalInputs.ReturnVerificationReport = new ReturnVerificationReport();
            request.OptionalInputs.ReturnVerificationReport.IncludeVerifier = true;
            request.OptionalInputs.ReturnVerificationReport.IncludeCertificateValues = true;

            request.InputDocuments = new InputDocuments();
            request.InputDocuments.Document = new DocumentType[1];
            request.InputDocuments.Document[0] = new DocumentType();
            request.InputDocuments.Document[0].ID = document.Id;
            request.InputDocuments.Document[0].Base64Data = new Base64Data();
            request.InputDocuments.Document[0].Base64Data.MimeType = document.MimeType;

            memCopy.Wait();
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
