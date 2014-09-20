using EContract.Dssp.Client.Proxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EContract.Dssp.Client
{
    public partial class DsspClient
    {
        /// <summary>
        /// Uploads a document to e-Contract, asynchronously.
        /// </summary>
        /// <see cref="UploadDocument"/>
        public async Task<DsspSession> UploadDocumentAsync(Document document)
        {
            byte[] clientNonce;
            if (document == null) throw new ArgumentNullException("document");

            var client = CreateDSSPClient();
            var request = CreateSignRequest(document, out clientNonce);
            signResponse1 responseWrapper = await client.signAsync(request);
            return ProcessSignResponse(responseWrapper.SignResponse, clientNonce);
        }

        /// <summary>
        /// Downloads the document that was uploaded before and signed via the BROWSER/POST protocol, asynchronously.
        /// </summary>
        /// <see cref="DownloadDocument"/>
        public async Task<Document> DownloadDocumentAsync(DsspSession session)
        {
            if (session == null) throw new ArgumentNullException("session");

            var client = CreateDSSPClient(session);
            var downloadRequest = CreateDownloadRequest(session);
            pendingRequestResponse downloadResponseWrapper = await client.pendingRequestAsync(downloadRequest);
            return ProcessDownloadResponse(downloadResponseWrapper.SignResponse);
        }

        /// <summary>
        /// Validates the provided document via the e-contract service, asynchronously.
        /// </summary>
        /// <see cref="Verify"/>
        public async Task<SecurityInfo> VerifyAsync(Document document)
        {
            if (document == null) throw new ArgumentNullException("document");

            var client = CreateDSSPClient();
            var request = CreateVerifyRequest(document);
            verifyResponse responseWrapper = await client.verifyAsync(request);
            return ProcessVerifyResponse(responseWrapper.VerifyResponse1);
        }
        
    }
}
