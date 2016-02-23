using dssp_demo.Models;

namespace dssp_demo.Services
{
    public class Configuration
    {
        private static Config instance = new Config() { AltMode = false,  Lanuage = "en", AppName = "egelke", AppPwd="egelke" };

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