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
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace EContract.Dssp.Client
{
    public class DsspSessionHttpSessionStore : IDsspSessionStore
    {
        private const string SessionKey = "DsspSessionHttpSessionStore";

        private Dictionary<string, DsspSession> store = new Dictionary<string, DsspSession>();

        public DsspSession Load(string id)
        {
            Dictionary<string, DsspSession> store = HttpContext.Current.Session[SessionKey] as Dictionary<string, DsspSession>;
            if (store == null) throw new InvalidOperationException("The session doesn't contain a document store");

            return store[id];
        }

        public void Store(DsspSession session)
        {
            Dictionary<string, DsspSession> store = HttpContext.Current.Session[SessionKey] as Dictionary<string, DsspSession>;
            if (store == null)
            {
                store = new Dictionary<string, DsspSession>();
                HttpContext.Current.Session[SessionKey] = store;
            }
            store.Add(session.Id, session);
        }

        public void Remove(string id)
        {
            Dictionary<string, DsspSession> store = HttpContext.Current.Session[SessionKey] as Dictionary<string, DsspSession>;

            if (store != null && store.ContainsKey(id)) store.Remove(id); //if it is empty, then the work is already done
        }
    }
}
