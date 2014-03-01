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
using System.IO;
using System.Linq;
using System.Text;

namespace EContract.Dssp.Client
{
    /// <summary>
    /// Represents a document that must be signed (or that is signed).
    /// </summary>
    public class Document
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public Document()
        {

        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="mimeType">The mime type, must be supported by e-contract</param>
        /// <param name="content">The (binary) content of the document</param>
        public Document(String mimeType, Stream content)
        {
            this.MimeType = mimeType;
            this.Content = content;
        }

        internal Document(DocumentType document)
        {
            this.MimeType = document.Base64Data.MimeType;
            this.Content = new MemoryStream(document.Base64Data.Value);
        }

        /// <summary>
        /// The mime type of the document, e.g. "application/pdf".
        /// </summary>
        public string MimeType { get; set; }

        /// <summary>
        /// The content of the document.
        /// </summary>
        public Stream Content { get; set; }
    }
}
