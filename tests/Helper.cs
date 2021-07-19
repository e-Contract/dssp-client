using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace EContract.Dssp.Client
{
    public static class Helper
    {

#if NET40_OR_GREATER

#else
        public static void CopyTo(this Stream i, Stream o)
        {
            int len;
            byte[] buffer = new byte[1024];

            while((len = i.Read(buffer, 0, 1024)) > 0)
            {
                o.Write(buffer, 0, len);
            }
        }
#endif
    }
}
