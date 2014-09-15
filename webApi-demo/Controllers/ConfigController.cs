using dssp_demo.Models;
using dssp_demo.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace dssp_demo.Controllers
{
    public class ConfigController : ApiController
    {
        private Configuration configuraton = new Configuration();

        public Config Get()
        {
            return configuraton.Current;
        }

        public void Put(Config value)
        {
            configuraton.Current = value;
        }
    }
}
