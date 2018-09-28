using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SunRise.CyberLock.ServerSide.DAL
{
    [Serializable]
    public class Settings
    {
        public String EndpointName;
        public String ServicePort;

        public Settings()
        {
            this.EndpointName = "CyberLock";
            this.ServicePort = "1234";
        }
    }
}
