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
using System.Collections;
using System.Text;
using log4net.ObjectRenderer;
using System.Collections.Generic;

namespace log4net.Util.Serializer
{
    /// <summary>
    /// A simpleton implementation of a JSON serializer to supplement 
    /// System.Web.Script.Serialization.JavaScriptSerializer of NET35
    /// </summary>
    /// <author>Robert Sevcik</author>
    public class JsonSerializer : ISerializer
    {
        #region statics
#if FRAMEWORK_3_5_OR_ABOVE && !CLIENT_PROFILE && !NETCF
        /// <summary>
        /// Which serializer will be used by default?
        /// </summary>
        /// <remarks>
        /// Creating JsonBuiltinSerializer here
        /// </remarks>
        public static ISerializerFactory DefaultSerializerFactory = new JsonBuiltinSerializer.Factory();
#else
        /// <summary>
        /// Which serializer will be used by default?
        /// </summary>
        /// <remarks>
        /// Creating JsonSerializer here
        /// </remarks>
        public static ISerializerFactory DefaultSerializerFactory = new JsonSerializer.Factory();
#endif

        /// <summary>
        /// JSON escaped characters
        /// </summary>
        public static readonly IDictionary<char, string> EscapedChars = new Dictionary<char, string>()
            {
                {'"',"\\\""},
                {'\\',"\\\\"},
                {'/',"\\/"},
                {'\b',"\\b"},
                {'\f',"\\f"},
                {'\n',"\\n"},
                {'\r',"\\r"},
                {'\t',"\\t"},                
            };
        #endregion

        /// <summary>
        /// RendererMap given by the layout
        /// </summary>
        public RendererMap Map { get; set; }

        /// <summary>
        /// Serialize <paramref name="obj"/> to a JSON string
        /// </summary>
        /// <param name="obj">object to serialize</param>
        /// <returns>JSON string</returns>
        public object Serialize(object obj)
        {
            var sb = new StringBuilder();

            Serialize(obj, sb);

            return sb.ToString();
        }

        /// <summary>
        /// Serialize any object into a string builder
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="sb"></param>
        protected virtual void Serialize(object obj, StringBuilder sb)
        {
            if (obj == null)
            {
                sb.Append("null");
            }
            else if (obj is IDictionary)
            {
                SerializeDictionary((IDictionary)obj, sb);
            }
            else if (obj is string)
            {
                SerializeString(obj, sb);
            }
            else if (obj is byte[])
            {
                var str = Encoding.UTF8.GetString((byte[])obj);
                SerializeString(obj, sb);
            }
            else if (obj is DateTime)
            {
                SerializeDateTime((DateTime)obj, sb);
            }
            else if (obj is TimeSpan)
            {
                SerializeTimeSpan((TimeSpan)obj, sb);
            }
            else if (obj.GetType().IsPrimitive)
            {
                SerializePrimitive(obj, sb);
            }
            else if (obj is IEnumerable)
            {
                SerializeArray((IEnumerable)obj, sb);
            }
            else
            {
                SerializeObject(obj, sb);
            }
        }

        /// <summary>
        /// Serialize date and time into a string builder
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="sb"></param>
        protected virtual void SerializeDateTime(DateTime obj, StringBuilder sb)
        {
            Serialize(obj.ToString("o"), sb);
        }

        /// <summary>
        /// Serialize time span into a string builder
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="sb"></param>
        protected virtual void SerializeTimeSpan(TimeSpan obj, StringBuilder sb)
        {
            Serialize(obj.TotalMilliseconds, sb);
        }

        /// <summary>
        /// Serialize int's, byte's, char's, bools and friends into a string builder
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="sb"></param>
        protected virtual void SerializePrimitive(object obj, StringBuilder sb)
        {
            sb.Append(obj);
        }

        /// <summary>
        /// Serialize a dictionary into a string builder
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="sb"></param>
        protected virtual void SerializeDictionary(IDictionary obj, StringBuilder sb)
        {
            sb.Append("{");

            bool first = true;

            foreach (DictionaryEntry entry in (IDictionary)obj)
            {
                if (first)
                    first = false;
                else
                    sb.Append(",");

                sb.AppendFormat(@"""{0}"":", entry.Key);

                Serialize(entry.Value, sb);
            }

            sb.Append("}");
        }

        /// <summary>
        /// Serialize enumerables into a string builder
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="sb"></param>
        protected virtual void SerializeArray(IEnumerable obj, StringBuilder sb)
        {
            sb.Append("[");

            bool first = true;

            foreach (var item in obj)
            {
                if (first)
                    first = false;
                else
                    sb.Append(",");

                Serialize(item, sb);
            }

            sb.Append("]");
        }

        /// <summary>
        /// Serialize an object (last resort) into a string builder
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="sb"></param>
        protected virtual void SerializeObject(Object obj, StringBuilder sb)
        {
            var rendered = Map == null
                        ? Convert.ToString(obj)
                        : Map.FindAndRender(obj)
                        ;

            SerializeString(rendered, sb);
        }

        /// <summary>
        /// Serialize escaped string into a string builder
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="sb"></param>
        protected virtual void SerializeString(object obj, StringBuilder sb)
        {
            var str = Convert.ToString(obj);

            sb.Append(@"""");

            foreach (var c in str)
            {

                string cstring;

                if (EscapedChars.TryGetValue(c, out cstring))
                    sb.Append(cstring);
                else if (c < 32 || c > 126)
                    sb.AppendFormat("\\u{0:X4}", (int)c);
                else
                    sb.Append(c);
            }

            sb.Append(@"""");
        }

        /// <summary>
        /// Creates JsonSerializer
        /// </summary>
        public class Factory : ISerializerFactory
        {
            /// <summary>
            /// Create JsonSerializer
            /// </summary>
            /// <param name="obj"></param>
            /// <param name="map"></param>
            /// <returns></returns>
            public ISerializer GetSerializer(object obj, RendererMap map)
            {
                var serializer = new JsonSerializer() { Map = map };

                return serializer;
            }
        }
    }


}
