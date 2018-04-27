using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Assert=NUnit.Framework.Assert;
using log4net.ext.json.xunit.General;

namespace log4net.ext.json.xunit.Util.Stamps
{
    public class StampingLogger_Basic : RepoTest
    {
        protected override string GetConfig()
        {
            return @"
                <log4net>                    
                  <loggerFactory type=""log4net.Util.Stamps.StampingLoggerFactory, log4net.Ext.Json"">
                  </loggerFactory>
                  <root>
                    <level value=""ALL"" />
                  </root>
                </log4net>";
        }

		protected override void RunTestLog(log4net.ILog log)
        {
            log.Info("Hola!");

            var events = GetEvents(log.Logger);

            Assert.AreEqual(1, events.Length, "events Count");

            var le = events.Single();

            Assert.IsNotNull(le, "loggingevent");

            Assert.IsNotEmpty(le.Properties, "loggingevent Properties");

            Assert.IsNotNull(le.Properties["stamp"], "loggingevent Properties has stamp");
        }
    }
}
