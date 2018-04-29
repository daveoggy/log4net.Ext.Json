
using System;
using System.Collections.Generic;
using System.IO;
using log4net;
using log4net.ObjectRenderer;
using Newtonsoft.Json;

[assembly: log4net.Config.XmlConfigurator(ConfigFile = "log4net.config", Watch = true)]

namespace play
{

    public class MainClass
    {
        public string Tornado { get; set; } = "Loo";

        private static readonly ILog log = LogManager.GetLogger(typeof(MainClass));

        public static void Main(string[] args)
        {
            log.Info(new Dictionary<string, object> { { "dt", DateTime.Now }, { "then", null }, { "any", new MainClass() } });
            log.Info(new { msg = "Hello World!", now = DateTime.Now, then = null as string });
        }
    }
}
