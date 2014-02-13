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
using System.Text;
using log4net.ObjectRenderer;

namespace log4net.Util.Serializer
{
    /// <summary>
    /// Create serializers
    /// </summary>
    /// <author>Robert Sevcik</author>
    public interface ISerializerFactory
    {
        /// <summary>
        /// Creates a <see cref="ISerializer"/> with a specific <paramref name="map"/>
        /// </summary>
        /// <param name="obj">object to get a serializer for</param>
        /// <param name="map">renderer map to consider</param>
        /// <returns>a serializer for <paramref name="obj"/></returns>
        ISerializer GetSerializer(object obj, RendererMap map);
    }
}
