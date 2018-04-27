using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Assert=NUnit.Framework.Assert;
using log4net.ext.json.xunit.General;

namespace log4net.ext.json.xunit.Util.Stamps
{
    public class StampingLogger_CustomStamps : RepoTest
    {
        protected override string GetConfig()
        {
            return @"
                <log4net>                    
                  <loggerFactory type=""log4net.Util.Stamps.StampingLoggerFactory, log4net.Ext.Json"">
                    <stamp type=""log4net.Util.Stamps.ValueStamp, log4net.Ext.Json"">
                      <name>stamp.value</name>
                      <value>CustomValue</value>
                    </stamp>
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

            Assert.AreEqual(1, le.Properties.Count, "loggingevent Properties count");

            Assert.IsNotNull(le.Properties["stamp.value"], "loggingevent Properties has stamp");

            Assert.AreEqual("CustomValue", le.Properties["stamp.value"], "loggingevent Properties has stamp.value");
        }
    }
}
