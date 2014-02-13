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
using log4net.Util.TypeConverters;

namespace log4net.Layout.Arrangements
{
    /// <summary>
    /// This <see cref="IArrangement"/> allows the organization of the members to be serialized.
    /// An option is recognised and processed by <see cref="ArrangementConverter.GetArrangement"/>
    /// </summary>
    /// <remarks>
    /// <para>
    /// It is the base of the <see cref="DefaultArrangement"/> class.
    /// </para>
    /// </remarks>
    /// <author>Robert Sevcik</author>
    public class OptionArrangement : NoArrangement
    {
        /// <summary>
        /// The arrangement option to be parsed by <see cref="ArrangementConverter.GetArrangement"/>.
        /// </summary>
        public string Arrangement { get; set; }

        /// <summary>
        /// Parse the Arrangement string and use the new arrangement instance.
        /// </summary>
        /// <param name="members"></param>
        public override void Arrange(IList<IMember> members)
        {
            var arrangement = ArrangementConverter.GetArrangement(Arrangement);
            if (arrangement != null)
            {
                arrangement.SetConverters(Converters);
                arrangement.Arrange(members);
            }
        }

        /// <summary>
        /// Set the <see cref="Arrangement"/> option
        /// </summary>
        /// <param name="value"></param>
        public override void SetOption(string value)
        {
            Arrangement = value;
        }
    }
}
