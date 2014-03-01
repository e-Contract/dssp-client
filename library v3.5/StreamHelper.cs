﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace EContract.Dssp.Client
{
    internal static class StreamHelper
    {

        public static void CopyTo(this Stream input, Stream output)
        {
            byte[] buffer = new byte[1024];
            int bytesRead;

            while ((bytesRead = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, bytesRead);
            }
        }
    }
}
