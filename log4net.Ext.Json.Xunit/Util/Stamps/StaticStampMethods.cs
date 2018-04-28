using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using System.Diagnostics;
using System.Globalization;
using System.Threading;

namespace log4net.Ext.Json.Xunit.Util.Stamps
{
    public class StaticStampMethods
    {
        [Fact]
        public void Init()
        {
            log4net.Util.Stamps.Stamp.Init();
        }

        [Fact]
        public void GetProcessId()
        {
            var pid = log4net.Util.Stamps.Stamp.GetProcessId();
            var actualpid = Process.GetCurrentProcess().Id;
            Assert.Equal(pid, actualpid);
        }

        [Fact]
        public void GetSequence()
        {
            var seq = log4net.Util.Stamps.Stamp.GetSequence();
            var seq2 = log4net.Util.Stamps.Stamp.GetSequence();
            // this will not hold should seq overflow long, then it becomes long.MinValue
            Assert.True(seq2 > seq, "sequence is a progressive number");
        }

        [Fact]
        public void GetSequenceOverflow()
        {
            log4net.Util.Stamps.Stamp.SetSequence(long.MaxValue - 1);
            var seq = log4net.Util.Stamps.Stamp.GetSequence();
            var seq2 = log4net.Util.Stamps.Stamp.GetSequence();
            Assert.Equal(long.MaxValue, seq);
            Assert.Equal(long.MinValue, seq2);
        }

        [Fact]
        public void GetSystemUpTime()
        {
            var before = Stopwatch.GetTimestamp();
            Thread.Sleep(1);
            var time = log4net.Util.Stamps.Stamp.GetSystemUpTime();
            Thread.Sleep(1);
            var after = Stopwatch.GetTimestamp();
            var epoch = DateTime.ParseExact("1970", "yyyy", CultureInfo.InvariantCulture);
            Assert.True(epoch.AddSeconds(time)> epoch, "sys up time is after epoch");
			Assert.True(epoch.AddSeconds(time)< DateTime.Now, "sys up time is after epoch");
			Assert.True(after> time * Stopwatch.Frequency, "sys up time is driven by stopwatch");
			Assert.True(before< time * Stopwatch.Frequency, "sys up time is driven by stopwatch");
        }

        [Theory]
		[InlineData(log4net.Util.Stamps.AgeReference.Now)]
		[InlineData(log4net.Util.Stamps.AgeReference.Epoch1970)]
		[InlineData(log4net.Util.Stamps.AgeReference.ApplicationStart)]
		[InlineData(log4net.Util.Stamps.AgeReference.SystemStart)]
        public void GetEpochMicroTime(log4net.Util.Stamps.AgeReference timeRef)
        {
            var time = log4net.Util.Stamps.Stamp.GetEpochTime(timeRef);

            if (timeRef == log4net.Util.Stamps.AgeReference.Epoch1970)
                Assert.True(0 == time, "time is 0 for epoch time");
            else
                Assert.True(time > 0, "time is > 0 for anything but epoch time");
		}

        [Theory]
		[InlineData(log4net.Util.Stamps.AgeReference.Now,log4net.Util.Stamps.AgeReference.Now)]
		[InlineData(log4net.Util.Stamps.AgeReference.Epoch1970,log4net.Util.Stamps.AgeReference.Epoch1970)]
		[InlineData(log4net.Util.Stamps.AgeReference.ApplicationStart,log4net.Util.Stamps.AgeReference.ApplicationStart)]
		[InlineData(log4net.Util.Stamps.AgeReference.SystemStart,log4net.Util.Stamps.AgeReference.SystemStart)]
        public void GetTimeStampValue(           
            log4net.Util.Stamps.AgeReference from,            
            log4net.Util.Stamps.AgeReference to)
        {
            var time = log4net.Util.Stamps.Stamp.GetTimeStampValue(
                                from
                                , to
                                , 0
                                , false);

            Assert.True(time.GetType().IsPrimitive, "value is primitive");
            
            if (from < to)
                Assert.True(time> 0, "time must be > 0 if from < to");
            else if (from > to)
                Assert.True(time< 0, "time must be < 0 if from > to");
            else if (from == log4net.Util.Stamps.AgeReference.Now)
                Assert.True(time> 0, "time must be > 0 if from == to == Now");
            else 
                Assert.True(0== time, "time must be == 0 if from == to");
        }

    }
}
