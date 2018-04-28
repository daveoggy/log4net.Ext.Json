using System;
using log4net;

[assembly: log4net.Config.XmlConfigurator(ConfigFile="log4net.config", Watch=true)]

namespace playground
{
    class MainClass
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(MainClass));

        public static void Main(string[] args)
        {
            log.Info("Hello World!");
        }
    }
}
