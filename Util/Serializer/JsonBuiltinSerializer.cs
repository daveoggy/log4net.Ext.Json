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

#if FRAMEWORK_3_5_OR_ABOVE && !CLIENT_PROFILE && !NETCF

using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using log4net.ObjectRenderer;

// from System.Web.Extensions.dll
using System.Web.Script.Serialization;

namespace log4net.Util.Serializer
{

    /// <summary>
    /// A wrapper to <see cref="JavaScriptSerializer"/> of NET35
    /// </summary>
    /// <author>Robert Sevcik</author>
    public class JsonBuiltinSerializer : ISerializer
    {
        /// <summary>
        /// The instance of JavaScriptSerializer to use
        /// </summary>
        public JavaScriptSerializer BuiltinSerializer = new JavaScriptSerializer();

        /// <summary>
        /// Serialize object using builtin JavaScriptSerializer
        /// </summary>
        /// <param name="obj">object to serialize</param>
        /// <returns>serialized data</returns>
        public object Serialize(object obj)
        {
            return BuiltinSerializer.Serialize(obj);
        }

        /// <summary>
        /// Creates <see cref="JsonBuiltinSerializer"/>
        /// </summary>
        public class Factory : ISerializerFactory
        {
            /// <summary>
            /// Creates <see cref="JsonBuiltinSerializer"/> with no regards to <paramref name="map"/>
            /// </summary>
            /// <param name="obj">object to get a serializer for</param>
            /// <param name="map">renderer map to consider</param>
            /// <returns>a serializer for <paramref name="obj"/></returns>
            public ISerializer GetSerializer(object obj, RendererMap map)
            {
                var serializer = new JsonBuiltinSerializer();
                /*  
                //TODO: Consider registering RendererMap lookup with custom serializer
                if (rendererMap.HasExtraRenderes)
                {
                    serializer = new JavaScriptSerializer();
                    serializer.RegisterConverters(new JavaScriptConverter[] { new CustomJSConverter(rendererMap) });
                }
                */
                return serializer;
            }
        }
    }
}

#endif
