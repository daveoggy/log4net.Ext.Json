using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net.ext.json.xunit.General;
using Xunit;
using Assert=NUnit.Framework.Assert;
using log4net.Core;
using System.Collections;

namespace log4net.ext.json.xunit.Log
{
    /// <summary>
    /// http://sourceforge.net/p/log4net-json/support-tickets/9/
    /// </summary>
    public class CustomProperty : RepoTest
    {
        class Custom
        {
            public string Text { get { return "Number " + Number; } }
            public int Number { get { return counter++; } }
            protected int counter;
        }

        protected override string GetConfig()
        {
            return @"<log4net>
                        <root>
                          <level value='DEBUG'/>
                          <appender-ref ref='TestAppender'/>
                        </root>

                        <appender name='TestAppender' type='log4net.ext.json.xunit.General.TestAppender, log4net.ext.json.xunit'>
                          <layout type='log4net.Layout.SerializedLayout, log4net.Ext.Json'>
                             <decorator type='log4net.Layout.Decorators.StandardTypesDecorator, log4net.Ext.Json' />
                             <member value='message' />
                             <member value='custom' />
                          </layout>
                        </appender>
                      </log4net>";
        }

		protected override void RunTestLog(log4net.ILog log)
        {
            log4net.GlobalContext.Properties["custom"] = new Custom();
            try
            {
                log.Info("First");
                log.Info("Second");

                var events = GetEventStrings(log.Logger);

                Assert.AreEqual(2, events.Length, "events Count");

                var le1 = events.First();
                var le2 = events.Last();

                Assert.IsNotNull(le1, "loggingevent 1");
                Assert.IsNotNull(le2, "loggingevent 2");

				Assert.AreEqual(@"{""message"":""First"",""custom"":{""Text"":""Number 0"",""Number"":1}}" + Environment.NewLine, le1, "le1 has structured message");
				Assert.AreEqual(@"{""message"":""Second"",""custom"":{""Text"":""Number 2"",""Number"":3}}" + Environment.NewLine, le2, "le2 has structured message");
            }
            finally
            {
                log4net.GlobalContext.Properties.Remove("custom");
            }
        }
    }
}

