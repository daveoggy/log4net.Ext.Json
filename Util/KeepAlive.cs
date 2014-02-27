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
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using log4net.Appender;
using log4net.Core;
using log4net.Repository;

namespace log4net.Util
{
    /// <summary>
    /// Keep appenders and logging busy with occasional "Alive" notice
    /// </summary>
    /// <remarks>
    /// <para>
    /// This may serve to:
    /// 
    /// * force roll over of files even with little logging,
    /// * maintain and track application instances health
    /// 
    /// </para>
    /// <para>
    /// A single thread does the time scheduling and calling. 
    /// It is implemented as a static singleton. 
    /// Appenders can be <see cref="Manage"/>d and <see cref="Release"/>d.
    /// When at least one appender is managed, thread executes. Otherwise it stops.
    /// </para>
    /// <para>
    /// </para>
    /// It's made to be thread safe. 
    /// The code locks 'this' to synchronize, wait and pulse.
    /// Additionally the code locks the <see cref="m_calls"/> for any operation with appenders.
    /// </remarks>
    /// <author>Robert Sevcik</author>
    public sealed class KeepAlive : IDisposable
    {
        /// <summary>
        /// The only single static instance of this class
        /// </summary>
        public static readonly KeepAlive Instance = new KeepAlive();

        /// <summary>
        /// Let a callback be called by KeepAlive regularly;
        /// </summary>
        /// <param name="alivecall">callback to be called</param>
        /// <param name="interval">how often</param>
        public void Manage(AliveCall alivecall, int interval)
        {
            lock (this)
            {
                lock (m_calls)
                {
                    m_calls[alivecall] = MakeConfig(interval);

                    if (!m_run) Start();
                }

                Monitor.PulseAll(this);
            }
        }

        /// <summary>
        /// Stop managing an appender
        /// </summary>
        /// <param name="alivecall">callback to be released</param>
        public void Release(AliveCall alivecall)
        {
            lock (this)
            {
                lock (m_calls)
                {
                    m_calls.Remove(alivecall);

                    if (m_calls.Count == 0) Stop();
                }

                Monitor.PulseAll(this);
            }
        }

        /// <summary>
        /// Internal appender and config store
        /// </summary>
        readonly IDictionary<AliveCall, Config> m_calls = new Dictionary<AliveCall, Config>();

        /// <summary>
        /// Repository used for custom <see cref="LoggingEvent"/>
        /// </summary>
        ILoggerRepository m_rep;

        /// <summary>
        /// The Alive thread
        /// </summary>
        Thread m_thread;

        /// <summary>
        /// flag indication that <see cref="Run"/> should continue.
        /// </summary>
        bool m_run;

        /// <summary>
        /// Exit the <see cref="Run"/> loop
        /// </summary>
        private void Stop()
        {
            lock (this)
            {
                m_run = false;
                Monitor.PulseAll(this);

                if (m_thread == null) return;
                if (m_thread.ThreadState == ThreadState.Unstarted) return;

                if (!m_thread.Join(100))
                {
                    m_thread.Interrupt();

                    if (!m_thread.Join(500))
                        m_thread.Abort();
                }
            }
        }

        /// <summary>
        /// Initiate the <see cref="Run"/> loop
        /// </summary>
        private void Start()
        {
            lock (this)
            {
                Stop();

                m_rep = LogManager.GetRepository(typeof(KeepAlive).Assembly);

                m_thread = new Thread(Run);
                m_thread.Name = typeof(KeepAlive).FullName;
                m_thread.Priority = ThreadPriority.Lowest;
                m_thread.IsBackground = true;
                m_thread.Start();

                Monitor.PulseAll(this);
            }
        }

        /// <summary>
        /// Loop as long as <see cref="m_run"/>
        /// </summary>
        private void Run()
        {
            m_run = true;

            var refdt = DateTime.ParseExact("1970", "yyyy", CultureInfo.InvariantCulture);

            var exception_logged = false;

            while (m_run)
            {
                bool locked = false;

                try
                {
                    while (m_run && !(locked = Monitor.TryEnter(this, 1))) Thread.Sleep(1);

                    if (!m_run) break;

                    var now = DateTime.UtcNow;

                    var waketime = now.AddMinutes(1);

                    lock (m_calls)
                    {
                        foreach (var kvp in m_calls)
                        {
                            var call = kvp.Key;
                            var config = kvp.Value;

                            if (now >= config.Schedule)
                            {
                                call();
                            }

                            var wake_ms = (now - refdt).TotalMilliseconds + config.Interval + 10;
                            wake_ms -= wake_ms % config.Interval;
                            wake_ms = Math.Round(wake_ms) + config.Offset;

                            config.Schedule = refdt.AddMilliseconds(wake_ms);

                            if (waketime > config.Schedule) waketime = config.Schedule;
                        }
                    }

                    var snooze = TimeSpan.Zero;
                    while (0 < (snooze = waketime - DateTime.UtcNow).Ticks)
                    {
                        if (Monitor.Wait(this, snooze)) break;
                    }
                }
                catch (ThreadAbortException tax)
                {
                    m_run = false;

#if LOG4NET_1_2_10_COMPATIBLE
                    LogLog.Error("Alive.Run() aborting.", tax);
#else
                    LogLog.Error(GetType(), "Alive.Run() aborted.", tax);
#endif
                }
                catch (Exception x)
                {
                    if (!exception_logged)
                    {
                        exception_logged = true;

#if LOG4NET_1_2_10_COMPATIBLE
                        LogLog.Error("Exception in Alive.Run(). Further exceptions will not be logged.", x);
#else
                        LogLog.Error(GetType(), "Exception in Alive.Run(). Further exceptions will not be logged.", x);
#endif
                    }

                    // in case this is a fast failure, eliminate CPU grab
                    Thread.Sleep(10);
                }
                finally
                {
                    if (locked) Monitor.Exit(this);
                }
            }
        }

        void IDisposable.Dispose()
        {
            Stop();
        }

        Config MakeConfig(int interval)
        {
            var config = new Config() { Interval = interval, Schedule = DateTime.Today };

            if (config.Interval <= 0)
                config.Interval = 60000;

            while (config.Offset == 0)
                config.Offset = new Random((int)(DateTime.Now.Ticks % int.MaxValue)).Next(config.Interval);

            return config;
        }

        /// <summary>
        /// Per-appender config structure
        /// </summary>
        private struct Config
        {
            /// <summary>
            /// Interval to keep alive with
            /// </summary>
            public int Interval;

            /// <summary>
            /// Offset to the interval. 
            /// </summary>
            /// <remarks>
            /// Randomize the logging time so that if many apps log come together, they don't compete too much.
            /// Still keep the regular interval.
            /// </remarks>
            public int Offset;

            /// <summary>
            /// Next run schedule
            /// </summary>
            public DateTime Schedule;
        }

        public delegate void AliveCall();
    }
}
