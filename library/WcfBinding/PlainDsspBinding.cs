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
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Security;
using System.ServiceModel.Security.Tokens;
using System.Text;
using System.Threading.Tasks;

namespace EContract.Dssp.Client.WcfBinding
{
    internal class PlainDsspBinding : Binding
    {
        private SecurityBindingElement security;

        private MessageEncodingBindingElement messageEncoding;

        private TransportBindingElement transport;

        public PlainDsspBinding()
        {
            security = CreateSecurity();
            messageEncoding = CreateMessageEncoding();
            transport = CreateTransport();
        }

        public override BindingElementCollection CreateBindingElements()
        {
            BindingElementCollection elements = new BindingElementCollection();
            if (security != null) elements.Add(security);
            elements.Add(messageEncoding);
            elements.Add(transport);

            return elements.Clone();
        }

        protected virtual SecurityBindingElement CreateSecurity()
        {
            return null;
            
        }

        private MessageEncodingBindingElement CreateMessageEncoding()
        {
            TextMessageEncodingBindingElement encoding = new TextMessageEncodingBindingElement();
            encoding.MessageVersion = MessageVersion.Soap12;
            encoding.ReaderQuotas.MaxArrayLength = 1024 * 1024 * 1024; //1 GB
            encoding.ReaderQuotas.MaxStringContentLength = 1024 * 1024 * 1024; //1 GB
            return encoding;
        }

        private TransportBindingElement CreateTransport()
        {
            HttpsTransportBindingElement transport = new HttpsTransportBindingElement();
            transport.MaxReceivedMessageSize = 1024 * 1024 * 1024; //1 GB
            transport.MaxBufferSize = (int) transport.MaxReceivedMessageSize;
            transport.AuthenticationScheme = System.Net.AuthenticationSchemes.Anonymous;
            transport.HostNameComparisonMode = HostNameComparisonMode.WeakWildcard;
            

            return transport;
        }

        public override string Scheme
        {
            get { return "https"; }
        }
    }
}
