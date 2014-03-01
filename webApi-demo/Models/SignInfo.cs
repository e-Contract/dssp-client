using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace dssp_demo.Models
{
    public class SignInfo
    {
        public SignInfo(byte[] signRequest)
        {
            this.SignRequest = signRequest;
        }

        public byte[] SignRequest;
    }
}