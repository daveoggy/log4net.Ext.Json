using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net.Ext.Json.Xunit.General;
using Xunit;
using Assert=NUnit.Framework.Assert;
using StringAssert=NUnit.Framework.StringAssert;
using Is=NUnit.Framework.Is;

namespace log4net.Ext.Json.Xunit.Layout.Arrangements
{
    public class DefaultImplied : RepoTest
    {
        protected override string GetConfig()
        {
            return @"<log4net>
                        <root>
                          <level value='DEBUG'/>
                          <appender-ref ref='TestAppender'/>
                        </root>

                        <appender name='TestAppender' type='log4net.Ext.Json.Xunit.General.TestAppender, log4net.Ext.Json.Xunit'>
                          <layout type='log4net.Layout.SerializedLayout, log4net.Ext.Json'>
                          </layout>
                        </appender>
                      </log4net>";
        }

		protected override void RunTestLog(log4net.ILog log)
        {
            log.Info(4);

            var events = GetEventStrings(log.Logger);

            Assert.AreEqual(1, events.Length, "events Count");

            var le = events.Single();

            Assert.IsNotNull(le, "loggingevent");

            StringAssert.Contains(@"""date"":", le, "log line has date");
            StringAssert.Contains(@"""message"":", le, "log line has message");
            StringAssert.Contains(@"""logger"":", le, "log line has logger");
            StringAssert.Contains(@"""level"":", le, "log line has level");
        }
    }
}

