using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Assert=NUnit.Framework.Assert;
using Is=NUnit.Framework.Is;
using log4net.Core;
using System.Diagnostics;

namespace log4net.Ext.Json.Xunit.Util.Stamps
{
    public class StampTest
    {
        protected virtual LoggingEvent CreateLoggingEvent(Type logger = null, Level level = null, object message = null, Exception exception = null)
        {
            logger = logger ?? GetType();
            level = level ?? Level.Info;
            var repo = LogManager.GetRepository();
            var le = new LoggingEvent(logger, repo, logger.FullName, level, message, exception);
            return le;
        }

        [Fact]
        public void RegularStamp()
        {
            var le = CreateLoggingEvent();
            var stamp = new log4net.Util.Stamps.Stamp();
            var stamp2 = new log4net.Util.Stamps.Stamp() { Name = "stamp2" };
            stamp.StampEvent(le);
            stamp2.StampEvent(le);

            Assert.That(le.Properties["stamp"], Is.Not.Null, @"Properties[""stamp""]");
            Assert.That(le.Properties["stamp2"], Is.Not.Null, @"Properties[""stamp2""]");
            Assert.AreNotEqual(le.Properties["stamp"], le.Properties["stamp2"], @"stamp!=stamp2");
        }

        [Fact]
        public void TimeStamp()
        {
            var le = CreateLoggingEvent();
            var stamp = new log4net.Util.Stamps.TimeStamp();
            var stamp2 = new log4net.Util.Stamps.TimeStamp() { Name = "stamp2" };
            stamp.StampEvent(le);
            stamp2.StampEvent(le);

            Assert.That(le.Properties["stamp"], Is.Not.Null, @"Properties[""stamp""]");
            Assert.That(le.Properties["stamp2"], Is.Not.Null, @"Properties[""stamp2""]");
            Assert.Greater((double)le.Properties["stamp2"], (double)le.Properties["stamp"], @"stamp2 > stamp");
        }

        [Fact]
        public void TimeStampRound()
        {
            var le = CreateLoggingEvent();
            var stamp = new log4net.Util.Stamps.TimeStamp() { Round = true };
            stamp.StampEvent(le);
            var value = Convert.ToString(le.Properties["stamp"]);
            var time = 0L;
            Assert.IsTrue(long.TryParse(value, out time), "{0} must be a long when Round=true", value);
        }

        [Fact]
        public void SequenceStamp()
        {
            var le = CreateLoggingEvent();
            var stamp = new log4net.Util.Stamps.SequenceStamp();
            var stamp2 = new log4net.Util.Stamps.SequenceStamp() { Name = "stamp2" };
            stamp.StampEvent(le);
            stamp2.StampEvent(le);

            Assert.That(le.Properties["stamp"], Is.Not.Null, @"Properties[""stamp""]");
            Assert.That(le.Properties["stamp2"], Is.Not.Null, @"Properties[""stamp2""]");
            Assert.Greater((long)le.Properties["stamp2"], (long)le.Properties["stamp"], @"stamp2 > stamp");
        }

        [Fact]
        public void ProcessIdStamp()
        {
            var le = CreateLoggingEvent();
            var stamp = new log4net.Util.Stamps.ProcessIdStamp();
            var stamp2 = new log4net.Util.Stamps.ProcessIdStamp() { Name = "stamp2" };
            stamp.StampEvent(le);
            stamp2.StampEvent(le);

            Assert.That(le.Properties["stamp"], Is.Not.Null, @"Properties[""stamp""]");
            Assert.That(le.Properties["stamp2"], Is.Not.Null, @"Properties[""stamp2""]");
            Assert.AreEqual((int)le.Properties["stamp2"], (int)le.Properties["stamp"], @"stamp2 == stamp");
            Assert.AreEqual((int)le.Properties["stamp"], Process.GetCurrentProcess().Id, @"Process ID");
        }

        [Fact]
        public void ValueStamp()
        {
            var le = CreateLoggingEvent();
            var stamp = new log4net.Util.Stamps.ValueStamp() { Value = "A" };
            var stamp2 = new log4net.Util.Stamps.ValueStamp() { Name = "stamp2", Value = "B" };
            stamp.StampEvent(le);
            stamp2.StampEvent(le);

            Assert.That(le.Properties["stamp"], Is.Not.Null, @"Properties[""stamp""]");
            Assert.That(le.Properties["stamp2"], Is.Not.Null, @"Properties[""stamp2""]");
            Assert.AreEqual((string)le.Properties["stamp"], "A", @"stamp A");
            Assert.AreEqual((string)le.Properties["stamp2"], "B", @"stamp2 B");
        }
    }
}
