using EContract.Dssp.Client.Proxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EContract.Dssp.Client
{
    /// <summary>
    /// Requester authorization error.
    /// The signer was not authorized.
    /// </summary>
    public class AuthorizationError : RequestError
    {
        public NameIdentifierType AttemptedSigner { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">The exception message</param>
        public AuthorizationError(String message, NameIdentifierType attemptedSigner)
            : base(message)
        {
            this.AttemptedSigner = attemptedSigner;
        }
    }
}
