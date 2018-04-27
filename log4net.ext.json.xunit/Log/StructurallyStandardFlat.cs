using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net.ext.json.xunit.General;
using Xunit;
using Assert=NUnit.Framework.Assert;
using StringAssert=NUnit.Framework.StringAssert;
using log4net.Core;
using System.Collections;

namespace log4net.ext.json.xunit.Log
{
    public class StructurallyStandardFlat : RepoTest
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
                            <decorator type='log4net.Layout.Decorators.StandardTypesFlatDecorator, log4net.Ext.Json' />
                            <default />
                            <remove value='message' />
                            <member value='data:messageobject' />
                          </layout>
                        </appender>
                      </log4net>";
        }

		protected override void RunTestLog(log4net.ILog log)
        {
            log.Info(new { A = 1, B = new { X = "Y" } });

            var events = GetEventStrings(log.Logger);

            Assert.AreEqual(1, events.Length, "events Count");

            var le = events.Single();

            Assert.IsNotNull(le, "loggingevent");

            StringAssert.Contains(@"""data.A"":1", le, "le has structured message");
            StringAssert.Contains(@"""data.B.X"":""Y""", le, "le has structured message");

        }
    }
}

