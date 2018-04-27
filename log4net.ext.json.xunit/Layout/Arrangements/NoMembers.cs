using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net.ext.json.xunit.General;
using Xunit;

namespace log4net.ext.json.xunit.Layout.Arrangements
{
    public class NoMembers : RepoTest
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
                            <remove />
                          </layout>
                        </appender>
                      </log4net>"; 
        }

        protected override void RunTestLog(log4net.ILog log)
        {
            log.Info("Hola!");

            var events = GetEventStrings(log.Logger);

            Assert.Equal(1, events.Length/*, "events Count"*/);

            var le = events.Single();

            Assert.NotNull(le);

			Assert.Equal(@"""Hola!""" + Environment.NewLine, le /*, "log line has no members - just plain message"*/);
        }
    }
}

