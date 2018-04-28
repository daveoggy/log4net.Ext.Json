using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net.Ext.Json.Xunit.General;
using Xunit;
using Assert=NUnit.Framework.Assert;
using StringAssert=NUnit.Framework.StringAssert;
using log4net.Core;
using System.Collections;

namespace log4net.Ext.Json.Xunit.Log
{
    public class Contextually : RepoTest
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
                             <member value='ndc|%ndc' />
                             <member value='data' />
                             <member value='exception' />
                          </layout>
                        </appender>
                      </log4net>";
        }

		protected override void RunTestLog(log4net.ILog log)
        {
            using (log4net.ThreadContext.Stacks["NDC"].Push("TestLog"))
            {
                log4net.ThreadContext.Properties["data"] = new { A = 1, B = new { X = "Y" } };

                using (log4net.ThreadContext.Stacks["NDC"].Push("sub section"))
                {
                    log.Info("OK");
                }
            };

            var events = GetEventStrings(log.Logger);

            Assert.AreEqual(1, events.Length, "events Count");

            var le = events.Single();

            Assert.IsNotNull(le, "loggingevent");

            StringAssert.Contains(@"""data"":{", le, "le2 has structured message");
            StringAssert.Contains(@"""X"":""Y""", le, "le2 has structured message");
            StringAssert.Contains(@"""A"":1", le, "le2 has structured message");

            StringAssert.Contains(@"""TestLog sub section""", le, "le1 has structured message");

            StringAssert.DoesNotContain(@"""exception""", le, "le2 has no exception");
        }
    }
}

