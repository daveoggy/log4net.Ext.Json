#region Apache License
//
// Licensed to the Apache Software Foundation (ASF) under one or more 
// contributor license agreements. See the NOTICE file distributed with
// this work for additional information regarding copyright ownership. 
// The ASF licenses this file to you under the Apache License, Version 2.0
// (the "License"); you may not use this file except in compliance with 
// the License. You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#endregion

using System;
using System.Diagnostics;
using System.Globalization;

namespace log4net.Util.Stamps
{
    /// <summary>
    /// Set a time since unix epoch number property value on the event. 
    /// </summary>
    /// <remarks>
    /// It is seconds by default. This is a double precision value.
    /// It can be multiplied by <see cref="Multiplier"/> and <see cref="Round"/>ed.
    /// If the resulting value can be represented by a long type, long is returned, otherwise double.
    /// </remarks>
    /// <author>Robert Sevcik</author>
    public class PropertyTimeStamp : PropertyStamp
    {
        #region Statics

        /// <summary>
        /// lock root
        /// </summary>
        static protected readonly Object s_sync_root = new Object();

        /// <summary>
        /// System start reference time against unix epoch in seconds
        /// </summary>
        static double s_ref_sys_time;

        /// <summary>
        /// Application start reference time against unix epoch in seconds
        /// </summary>
        static double s_ref_app_time;

        /// <summary>
        /// Call <see cref="Init"/>
        /// </summary>
        static PropertyTimeStamp()
        {
            Init();
        }

        /// <summary>
        /// Initialize internal epoch time reference cache, thread safe
        /// </summary>
        public static void Init()
        {
            // init only if not done yet, thread safe

            if (s_ref_sys_time == 0)
                lock (s_sync_root)
                {
                    // Check if someone else was faster
                    if (s_ref_sys_time != 0) return;

                    // First we work with the NOW now, to get precise system start time

                    var now = DateTime.UtcNow;
                    var uptime = GetSystemUpTime();                    
                    var epoch = DateTime.ParseExact("1970", "yyyy", CultureInfo.InvariantCulture);
                    var espan = now - epoch;

                    s_ref_app_time = ConvertTimeSpanToSeconds(espan);
                    s_ref_sys_time = s_ref_app_time - uptime;

                    // Then try get the actual application start time. The 'NOW now' was possibly the time of the first logging event

                    try
                    {
                        espan = Process.GetCurrentProcess().StartTime - epoch;
                        s_ref_app_time = ConvertTimeSpanToSeconds(espan);
                    }
                    catch(Exception x)
                    {
#if LOG4NET_1_2_10_COMPATIBLE
                        LogLog.Warn("PropertyTimeStamp.Init() - getting exact app start time failed, but we should be fine with approximate time.", x);
#else
                        LogLog.Warn(typeof(PropertyTimeStamp), "PropertyTimeStamp.Init() - getting exact app start time failed, but we should be fine with approximate time.", x);
#endif
                    }
                }
        }

        /// <summary>
        /// Utility method returns current time in seconds since system start using the <see cref="Stopwatch.GetTimestamp"/>
        /// </summary>
        /// <returns>seconds</returns>
        public static double GetSystemUpTime()
        {
            return ConvertStopwatchTicksToSeconds(Stopwatch.GetTimestamp());
        }

        /// <summary>
        /// Utility method converts ticks given by <see cref="Stopwatch"/> to seconds
        /// </summary>
        /// <param name="ticks">Stopwatch ticks</param>
        /// <returns>seconds</returns>
        public static double ConvertStopwatchTicksToSeconds(long ticks)
        {
            return ((double)ticks) / Stopwatch.Frequency;
        }

        /// <summary>
        /// Utility method converts ticks given by <see cref="TimeSpan"/> to seconds
        /// </summary>
        /// <param name="span">TimeSpan</param>
        /// <returns>seconds</returns>
        public static double ConvertTimeSpanToSeconds(TimeSpan span)
        {
            return ((double)span.Ticks) / TimeSpan.TicksPerSecond;
        }

        #endregion Statics

        /// <summary>
        /// Round the double value to whole units
        /// </summary>
        public bool Round { get; set; }

        /// <summary>
        /// Change unit by multiplying the default seconds. For instance * 1000 for miliseconds.
        /// </summary>
        public double Multiplier { get; set; }

        /// <summary>
        /// The point of reference (Unix epoch (default), system start or application start)
        /// </summary>
        public AgeReference Reference { get; set; }

        /// <summary>
        /// Provide <see cref="PropertyStamp"/> with a epoch time number value
        /// </summary>
        /// <param name="loggingEvent">event to stamp</param>
        /// <returns>property value to set</returns>
        protected override object GetValue(Core.LoggingEvent loggingEvent)
        {
            double time;

            switch (Reference)
            {
                case AgeReference.SinceUnixEpoch: time = s_ref_sys_time + GetSystemUpTime(); break;
                case AgeReference.SystemUpTime: time = GetSystemUpTime(); break;
                case AgeReference.ApplicationUpTime: time = s_ref_app_time - s_ref_sys_time + GetSystemUpTime(); break;
                case AgeReference.SystemStart: time = s_ref_sys_time; break;
                case AgeReference.ApplicationStart: time = s_ref_app_time; break;
                default: throw new NotImplementedException(String.Format("AgeReference not implemented: {0}", Reference));
            }

            var value = time;

            if (Multiplier != 0 && Multiplier != 1)
            {
                value = value * Multiplier;
            }

            if (Round)
            {
                value = Math.Round(value);
            }

            if (value <= long.MaxValue && value >= long.MinValue && (value % 1) == 0)
            {
                // if the value can be represented by long, make it long
                return (long)value;
            }
            else
            {
                return value;
            }
        }

    }
}
