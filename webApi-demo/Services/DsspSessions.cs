using EContract.Dssp.Client;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace dssp_demo.Services
{
    public class DsspSessions
    {
        //Demo only, this should be an actual document store of some sorts
        private static ConcurrentDictionary<String, DsspSession> sessions = new ConcurrentDictionary<String, DsspSession>();

        public DsspSession this[string id]
        {
            get
            {
                return sessions[id];
            }
            set
            {
                sessions[id] = value;
            }
        }
    }
}