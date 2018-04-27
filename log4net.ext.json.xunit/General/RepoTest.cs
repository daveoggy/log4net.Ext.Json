using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Assert=NUnit.Framework.Assert;
using Is=NUnit.Framework.Is;
using System.Xml;
using log4net.Config;

namespace log4net.Ext.Json.Xunit.General
{
    public class RepoTest
    {
        protected log4net.Repository.ILoggerRepository repo;

		public RepoTest()
        {
            var config = GetConfig();
            XmlDocument log4netConfig = new XmlDocument();
            log4netConfig.LoadXml(config);

            var rep = LogManager.CreateRepository(Guid.NewGuid().ToString()) as log4net.Repository.Hierarchy.Hierarchy;
            XmlConfigurator.Configure(rep, log4netConfig["log4net"]);
            
            if(rep.GetAppenders().Length == 0)
                rep.Root.AddAppender(new TestAppender() { Name = "TestAppender" });

            repo = rep;
        }

        [Fact]
        public virtual void TestLog()
        {
            var log = GetLog();

            Assert.IsNotNull(log);

            RunTestLog(log);
        }

        protected virtual void RunTestLog(log4net.ILog log)
        {
            log.Info("Hola!");

            var events = GetEvents(log.Logger);

            Assert.AreEqual(1, events.Length, "events Count");

            var le = events.Single();

            Assert.IsNotNull(le, "loggingevent");

            Assert.IsEmpty(le.Properties, "loggingevent Properties");
        }

        protected virtual log4net.ILog GetLog()
        {
            return LogManager.GetLogger(repo.Name, GetType());
            //var logger = repo.GetLogger(GetType().FullName);
            //return logger;
        }

        protected virtual log4net.Core.LoggingEvent[] GetEvents(log4net.Core.ILogger logger)
        {
            var events = GetAppenders<TestAppender>(logger)
                            .SelectMany(ap=>ap.Events)
                            .ToArray();
            return events;
        }

        protected virtual String[] GetEventStrings(log4net.Core.ILogger logger)
        {
            var events = GetAppenders<TestAppender>(logger)
                            .SelectMany(ap => ap.EventStrings)
                            .ToArray();
            return events;
        }

        protected virtual T[] GetAppenders<T>(log4net.Core.ILogger logger) where T : log4net.Appender.IAppender
        {
            var loggerImpl = logger as log4net.Repository.Hierarchy.Logger;

            var appenders = new List<T>();

            for (var c = loggerImpl; c != null; c = c.Parent)
            {
                if (c.Appenders != null)
                {
                        appenders.AddRange(c.Appenders.OfType<T>());                    
                }

                if (!c.Additivity)
                {
                    break;
                }
            }

            return appenders.ToArray();
        }

        protected virtual string GetConfig()
        {
            return @"
                <log4net>
                  <root>
                    <level value=""ALL"" />
                  </root>
                </log4net>";
        }

    }
}
