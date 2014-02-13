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
    /// Here we merely allow multiple arrangements to be represented by a single object.
    /// </summary>
    /// <remarks>
    /// <para>
    /// It's used by <see cref="SerializedLayout"/> and <see cref="ArrangementConverter"/> internally.
    /// </para>
    /// </remarks>
    /// <author>Robert Sevcik</author>
    public class MultipleArrangement : NoArrangement
    {
        /// <summary>
        /// All arrangements collected by <see cref="AddArrangement"/>
        /// </summary>
        public IList<IArrangement> Arrangements { get; protected set; }

        /// <summary>
        /// Create instance with <see cref="Arrangements"/> set
        /// </summary>
        public MultipleArrangement()
        {
            Arrangements = new List<IArrangement>();
        }

        /// <summary>
        /// Simply call each and every one of the <see cref="Arrangements"/>
        /// </summary>
        /// <param name="members">Members to be arranged</param>
        public override void Arrange(IList<IMember> members)
        {
            foreach (var arrangement in Arrangements)
            {
                if (arrangement == null) continue;
                arrangement.SetConverters(Converters);
                arrangement.Arrange(members);
            }
        }

        /// <summary>
        /// Parse the option as arrangement and add it to the list
        /// </summary>
        /// <param name="value">The option understood by <see cref="ArrangementConverter.GetArrangement"/></param>
        public override void SetOption(string value)
        {
            AddArrangement(ArrangementConverter.GetArrangement(value));
        }

        /// <summary>
        /// Well, add an arrangement
        /// </summary>
        /// <param name="arrangement">Arrangement to add</param>
        public virtual void AddArrangement(IArrangement arrangement)
        {
            Arrangements.Add(arrangement);
        }
    }
}
