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

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EContract.Dssp.Client
{
    [Serializable]
    public class DsspSession
    {
        internal static DsspSession New(string documentId)
        {
            return new DsspSession("uuid:" + Guid.NewGuid().ToString(), documentId);
        }

        /// <summary>
        /// Alias for the client ID.
        /// </summary>
        [NotMapped]
        public string Id
        {
            get
            {
                return ClientId;
            }
        }

        public DsspSession()
        {

        }

        internal DsspSession(string clientId, string documentId)
        {
            this.ClientId = clientId;
            this.DocumentId = documentId;
        }

        public DsspSession(string clientId, string serverId, string keyId, byte[] keyValue, string documentId)
        {
            this.ClientId = clientId;
            this.ServerId = serverId;
            this.KeyId = keyId;
            this.KeyValue = keyValue;
            this.DocumentId = documentId;
        }

        /// <summary>
        /// The session ID, client size
        /// </summary>
        [Key]
        [MaxLength(128)]
        public string ClientId { get; set; }

        /// <summary>
        /// The session ID, server side
        /// </summary>
        [MaxLength(128)]
        public string ServerId { get; set; }

        /// <summary>
        /// The ID of the session key (aka token).
        /// </summary>
        [MaxLength(128)]
        public string KeyId { get; set; }

        /// <summary>
        /// The value of the session key.
        /// </summary>
        [MaxLength(512)]
        public byte[] KeyValue { get; set; }

        /// <summary>
        /// The ID of the document that was signed.
        /// </summary>
        /// <remarks>
        /// The information is optional, and may be omitted from the store.
        /// </remarks>
        [MaxLength(128)]
        public String DocumentId { get; set; }
    }
}
