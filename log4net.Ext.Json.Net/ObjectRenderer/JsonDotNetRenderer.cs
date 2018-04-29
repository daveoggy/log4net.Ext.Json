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
using System.IO;
using Newtonsoft.Json;

namespace log4net.ObjectRenderer
{
    /// <summary>
    /// Use the Newtonsoft.Json serializer to render log4net output
    /// </summary>
    /// <author>Robert Cutajar</author>
    public class JsonDotNetRenderer : JsonSerializerSettings, IJsonRenderer
    {
        /// <summary>
        /// Serialize value into text writer
        /// </summary>
        /// <remarks>
        /// I've tried and failed to support the RendereMap here in some 
        /// decent way. Pull requests welcome. Until then, map is ignored.
        /// </remarks>
        /// <param name="rendererMap">Renderer map - ignored</param>
        /// <param name="value">Value to be serialized</param>
        /// <param name="writer">Where JSON will be written</param>
        public void RenderObject(RendererMap rendererMap, object value, TextWriter writer)
        {
            var json = Serialize(value, rendererMap);
            writer.Write(json);
        }

        protected virtual string Serialize(object obj, RendererMap map)
        {
            var type = GetSerializedType(obj);
            var settings = GetSettings(obj, type, map);
            return JsonConvert.SerializeObject(obj, type, settings);
        }

        /// <summary>
        /// Get type to be serialized
        /// </summary>
        /// <param name="obj">Value to be serialized</param>
        /// <returns>Type to be serialized</returns>
        protected virtual Type GetSerializedType(object obj)
            => obj == null ? typeof(void) : obj.GetType();

        /// <summary>
        /// Get Json.NET settings 
        /// </summary>
        /// <remarks>
        /// This object itself is the settings. RendererMap is not supported.
        /// </remarks>
        /// <param name="obj">Value to be serialized</param>
        /// <param name="type">Type to serialize as</param>
        /// <param name="map">Renderer map</param>
        /// <returns>Settings for this call</returns>
        protected virtual JsonSerializerSettings GetSettings(object obj, Type type, RendererMap map)
            => this;
    }
}
