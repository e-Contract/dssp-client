using EContract.Dssp.Client;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

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

        public DsspSession Remove(string id)
        {
            DsspSession session;

            sessions.TryRemove(id, out session);

            return session;
        }

        //This must be called periodically, not done for the demo
        public void Cleanup()
        {
            foreach (KeyValuePair<string, DsspSession> session in sessions)
            {
                if (session.Value.ExpiresOn < DateTime.UtcNow.AddMinutes(-5))
                {
                    this.Remove(session.Key);
                }
            }
        }
    }
}