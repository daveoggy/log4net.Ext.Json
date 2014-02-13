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
using log4net.Layout.Members;
using log4net.Util.TypeConverters;

namespace log4net.Layout.Arrangements
{
    /// <summary>
    /// This <see cref="IArrangement"/> will put together few most obvious values as defaults.
    /// These <see cref="ConfigDefaults"/> are the options recognized by <see cref="ArrangementConverter"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// If no other arrangement is set for the <see cref="SerializedLayout"/> it will add a default default by default.
    /// </para>
    /// <para>
    /// It is used by <see cref="SerializedLayout.AddDefault"/> to allow simple xml configuration 
    /// &lt;default value="nxlog" /&gt; or simply &lt;default /&gt;.
    /// </para>
    /// <para>
    /// It is used by <see cref="ArrangementConverter" /> to represent "DEFAULT:nxlog" or simply "DEFAULT" 
    /// in the serialize conversion pattern option.
    /// </para>
    /// <para>
    /// The arrangement is actually done by the base <see cref="OptionArrangement"/> implementation.
    /// </para>
    /// </remarks>
    /// <author>Robert Sevcik</author>
    public class DefaultArrangement : OptionArrangement
    {
        /// <summary>
        /// This is the default <see cref="Default"/> containing "default" :o)
        /// </summary>
        public const string DefaultDefaultDefault = "default";

        #region Static defaults

        /// <summary>
        /// A dictionary of default options which are recognized by <see cref="ArrangementConverter"/>
        /// </summary>
        public static IDictionary<string, string> ConfigDefaults
                = new Dictionary<string, string>()
                    {
                        {"default",
                            "Date|%date{o};"
                            + "Level;"
                            + "AppDomain;"
                            + "Logger;"
                            + "Thread;"
                            + "Message;"
                            + "Exception;"                            
                        },
                        {"nxlog",
                            "EventTime|%date{o};"
                            + "Severity:level;"
                            + "SourceName:appdomain;"
                            + "Logger;"
                            + "Thread;"
                            + "Message;"
                            + "Exception;"                                                                     
                        }
                    };

        #endregion

        #region Properties

        /// <summary>
        /// Default option of <see cref="Config"/>
        /// </summary>
        public string Default { get; set; }

        /// <summary>
        /// Default values configuration
        /// </summary>
        public IDictionary<string, string> Config { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Create an instance with <see cref="Default"/> = "default".
        /// Copy static <see cref="ConfigDefaults"/> dictionary to <see cref="Config"/>
        /// </summary>
        public DefaultArrangement()
            : this(null)
        {
        }

        /// <summary>
        /// Create an instance with specific <see cref="Default"/>.
        /// Copy static <see cref="ConfigDefaults"/> dictionary to <see cref="Config"/>
        /// </summary>
        public DefaultArrangement(string def)
        {
            Default = def ?? DefaultDefaultDefault;
            Config = new Dictionary<string,string>(ConfigDefaults);
        }

        #endregion

        #region Implementation of IArrangement, OptionArrangement overrides

        /// <summary>
        /// This implementation will pick a <see cref="Default"/> from <see cref="Config"/>
        /// and call the base <see cref="OptionArrangement"/> implementation on that
        /// </summary>
        /// <exception cref="Exception">When the <see cref="Default"/> is not found in <see cref="Config"/></exception>
        /// <param name="members">Members to be arranged</param>
        public override void Arrange(IList<IMember> members)
        {
            string arrangement;

            if (Default == null)
            {
                // null is fine, means we'll do nothing much
                arrangement = null;
            }
            else if (!Config.TryGetValue(Default, out arrangement))
            {
                // if a Default is set, it must be present in Config
                throw new Exception(String.Format("Defaults not found for: '{0}'", Default));
            }

            // update base option
            base.SetOption(arrangement);

            // actuall arrangement is done by the base implementation
            base.Arrange(members);
        }

        /// <summary>
        /// Chose the <see cref="Default"/> of <see cref="Config"/>
        /// </summary>
        /// <remarks>
        /// base.SetOption(value) is called from <see cref="Arrange"/>
        /// </remarks>
        /// <param name="value">Config dictionary key</param>
        public override void SetOption(string value)
        {
            Default = value;
        }

        #endregion

    }
}
