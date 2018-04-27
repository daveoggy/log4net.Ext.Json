using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net.ext.json.xunit.General;
using Xunit;
using Assert=NUnit.Framework.Assert;
using Is=NUnit.Framework.Is;

namespace log4net.ext.json.xunit.Layout.Arrangements
{
    public class MessageObject : RepoTest
    {
        protected override string GetConfig()
        {
            return @"<log4net>
                        <root>
                          <level value='DEBUG'/>
                          <appender-ref ref='TestAppender'/>
                        </root>

                        <appender name='TestAppender' type='log4net.ext.json.xunit.General.TestAppender, log4net.ext.json.xunit'>
                          <layout type='log4net.Layout.SerializedLayout, log4net.Ext.Json'>
                            <member value='MessageObject' />
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

			Assert.AreEqual(@"{""MessageObject"":4}" + Environment.NewLine, le, "log line");
        }
    }
}