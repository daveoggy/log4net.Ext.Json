using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net.Ext.Json.Xunit.General;
using Xunit;
using Assert=NUnit.Framework.Assert;
using StringAssert=NUnit.Framework.StringAssert;
using Is=NUnit.Framework.Is;
using System.Diagnostics;

namespace log4net.Ext.Json.Xunit.Layout.Arrangements
{
    public class Members : RepoTest
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
                            <member value='OurCompany.ApplicationName' /> <!-- ref to property -->
                            <member value='A|L-%p-%c' /> <!-- (|) arbitrary pattern layout format -->
                            <member value='B%date:yyyy' /> <!-- (%:) one pattern layout conversion pattern with optional option -->
                            <member value='Host=ProcessId\;HostName' /> <!-- (=) nested structure, escape ; -->
                            <member value='App:appname' /> <!-- named member -->
                          </layout>
                        </appender>
                      </log4net>";
        }

		protected override void RunTestLog(log4net.ILog log)
        {
            log4net.GlobalContext.Properties["OurCompany.ApplicationName"] = "fubar";

            log.Info(4);

            var events = GetEventStrings(log.Logger);

            Assert.AreEqual(1, events.Length, "events Count");

            var le = events.Single();

            Assert.IsNotNull(le, "loggingevent");

            var procid = Process.GetCurrentProcess().Id;

            StringAssert.StartsWith(@"{""OurCompany.ApplicationName"":""fubar""", le, "log line");
            StringAssert.Contains(@",""Host"":{", le, "log line");
            StringAssert.Contains(@"""ProcessId"":" + procid, le, "log line");
            StringAssert.Contains(@"""HostName"":""" + Environment.MachineName + @"""", le, "log line");
            StringAssert.Contains(@"""A"":""L-INFO-log4net.Ext.Json.Xunit.Layout.Arrangements.Members""", le, "log line");
            StringAssert.Contains(@"""B"":""" + DateTime.Now.Year + @"""", le, "log line");
            StringAssert.Contains(@"""App"":""", le, "log line");
        }
    }
}