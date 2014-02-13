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

using System.Collections.Generic;
using log4net.Layout.Members;
using log4net.Util;
using log4net.Util.TypeConverters;

#if LOG4NET_1_2_10_COMPATIBLE
using ConverterInfo = log4net.Layout.PatternLayout.ConverterInfo;
#endif

namespace log4net.Layout.Arrangements
{
    /// <summary>
    /// This <see cref="IArrangement"/> represents no arrangements intended
    /// which is returned instead of null from <see cref="ArrangementConverter.GetArrangement"/>. 
    /// </summary>
    /// <remarks>
    /// <para>
    /// It is statically cached in <see cref="Instance"/>.
    /// </para>
    /// <para>
    /// It is used as a base for other implementations.
    /// </para>
    /// </remarks>
    /// <author>Robert Sevcik</author>
    public class NoArrangement : IArrangement
    {
        /// <summary>
        /// A static instance cache of the class
        /// </summary>
        public static IArrangement Instance = new NoArrangement();

        /// <summary>
        /// See <see cref="SetConverters"/>. This may be null.
        /// </summary>
        public ConverterInfo[] Converters { get; set; }

        #region Implementation of IArrangement
        
        /// <summary>
        /// Organize the <see cref="IMember"/>s to be serialized
        /// </summary>
        /// <remarks>By default, do nothing, which should be overriden by child class</remarks>
        /// <param name="members">Members to be arranged</param>
        public virtual void Arrange(IList<IMember> members)
        {
            // make no arrangements here
        }

        /// <summary>
        /// Converters should be passed down the chain from the hosting <see cref="SerializedLayout"/>.
        /// These are then used to render members. They are stored in <see cref="Converters"/>.
        /// </summary>
        /// <param name="converters"></param>
        public virtual void SetConverters(params ConverterInfo[] converters)
        {
            Converters = converters;
        }

        /// <summary>
        /// All arrangements can take an option which is handy for XML configuration and to simplify set up.
        /// </summary>
        /// <remarks>By default, there's no option taken, which should be overriden by child class</remarks>
        /// <param name="value">The option specific to the arrangement implementation</param>
        public virtual void SetOption(string value)
        {
            // no option taken
        }

        #endregion

    }
}
