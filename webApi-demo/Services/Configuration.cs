using dssp_demo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace dssp_demo.Services
{
    public class Configuration
    {
        private static Config instance = new Config() { Lanuage = "en", AppName = "egelke", AppPwd="egelke" };

        public Config Current
        {
            get
            {
                return instance;
            }
            set
            {
                instance = value;
            }
        }
    }
}