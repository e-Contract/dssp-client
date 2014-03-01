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
using System.Threading.Tasks;

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
        /// Constructor with basic info.
        /// </summary>
        /// <param name="id">The id of the document, mustbe XSD NCName compliant</param>
        /// <param name="mimeType">The mime type, must be supported by e-contract</param>
        /// <param name="content">The (binary) content of the document</param>
        public Document(String id, String mimeType, Stream content)
        {
            this.Id = id;
            this.MimeType = mimeType;
            this.Content = content;
        }

        /// <summary>
        /// Full constructor with all info.
        /// </summary>
        /// <param name="id">The id of the document, mustbe XSD NCName compliant</param>
        /// <param name="mimeType">The mime type, must be supported by e-contract</param>
        /// <param name="content">The (binary) content of the document</param>
        /// <param name="lang">The language of the document, will be used as UI language by e-contract</param>
        public Document(String id, String mimeType, Stream content, String lang)
            : this(id, mimeType, content)
        {
            this.Language = lang;
        }

        /// <summary>
        /// The id of the document to be signed, should by NCName compliant and not more then 80 characters.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The mime type of the document, e.g. application/pdf.
        /// </summary>
        public string MimeType { get; set; }

        /// <summary>
        /// The content of the document.
        /// </summary>
        public Stream Content { get; set; }

        /// <summary>
        /// Optional lanuage of the document, the e-contract UI will match this language if supported.
        /// </summary>
        public string Language { get; set; }
    }
}
