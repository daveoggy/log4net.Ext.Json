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
using log4net.Core;

namespace log4net.Util.Stamps
{
    /// <summary>
    /// Set a named property value on the event. Actual value must be provided by inheriting class.
    /// </summary>
    /// <author>Robert Sevcik</author>
    public abstract class PropertyStamp : IStamp
    {
        /// <summary>
        /// Property name to set
        /// </summary>
        public virtual String Name { get; set; }
        
        /// <summary>
        /// Verify all the requirements and set a named property value - stamp the event.
        /// </summary>
        /// <param name="loggingEvent">event to stamp</param>
        /// <remarks>
        /// Only primitive values are taken, otherwise stringified. This avoids late reference change problem.
        /// </remarks>
        public void StampEvent(Core.LoggingEvent loggingEvent)
        {
            var value = GetValue(loggingEvent);

            if(value == null) return;

            if(value is IFixingRequired)
                value = ((IFixingRequired)value).GetFixedObject();

            if(value == null) return;

            if(!(value is string || value.GetType().IsPrimitive))
                value = loggingEvent.Repository.RendererMap.FindAndRender(value);

            loggingEvent.Properties[Name] = value;
        }

        /// <summary>
        /// Provide <see cref="PropertyStamp"/> with a property value
        /// </summary>
        /// <param name="loggingEvent">event to stamp</param>
        /// <returns>property value to set</returns>
        protected abstract Object GetValue(Core.LoggingEvent loggingEvent);
    }
}
