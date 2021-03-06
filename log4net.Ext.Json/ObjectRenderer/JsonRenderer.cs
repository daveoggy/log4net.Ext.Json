﻿#region Apache License
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
using System.Reflection;
using log4net.Util;
using System.IO;

namespace log4net.ObjectRenderer
{
    /// <summary>
    /// A JSON serializer that makes use of the RendererMap to override serialization with another ISerializer
    /// </summary>
    /// <remarks>
    /// Override rendering by adding a custom <see cref="IJsonRenderer"> implementation to the RendererMap
    /// </remarks>
    /// <author>Robert Sevcik</author>
    public class JsonRenderer : IJsonRenderer
    {
        #region statics

        /// <summary>
        /// The bare minimal default serializer - static cache
        /// </summary>
		public static readonly IJsonRenderer Default = new JsonRenderer();

        #endregion

        /// <summary>
        /// JSON escaped characters
        /// </summary>
        public IDictionary<char, string> EscapedChars { get; set; } = new Dictionary<char, string>()
            {
                {'"',"\\\""},
                {'\\',"\\\\"},
                {'\b',"\\b"},
                {'\f',"\\f"},
                {'\n',"\\n"},
                {'\r',"\\r"},
                {'\t',"\\t"}, 
                // forward slash could be escaped instead of <> to allow javascript embedding
                //{'/',"\\/"},
                // but possible to do it like this instead:
                //{'<',"\\u003c"},
                //{'>',"\\u003e"},
                //{'&',"\\u0026"},
            };

        /// <summary>
        /// preserve object type in serialization. true => visible types, false => never. Default: false
        /// </summary>
        public bool SaveType { get; set; }
        /// <summary>
        /// preserve object type in serialization. true => also invisible types, false => only visible. Default: false
        /// </summary>
        public bool SaveInternalType { get; set; }

        /// <summary>
        /// Call ToString and save the string. Default false.
        /// </summary>
        public bool Stringify { get; set; }

        /// <summary>
        /// if <see cref="SaveType"/> then this is the name it will be saved as. Default "__type".
        /// </summary>
        public string TypeMemberName { get; set; } = "__type";

        /// <summary>
        /// if <see cref="Stringify"/> then this is the name it will be saved as.null Default "String".
        /// </summary>
        public string StringMemberName { get; set; } = "String";

        /// <summary>
        /// Create instance with defaults
        /// </summary>
        public JsonRenderer()
        {
        }

        /// <summary>
        /// Renders the object as JSON.
        /// </summary>
        /// <param name="rendererMap">Renderer map for rendering overrides.</param>
        /// <param name="obj">Object to be serialized.</param>
        /// <param name="writer">Writer to write to.</param>
        public void RenderObject(RendererMap rendererMap, object obj, TextWriter writer)
        {
            Serialize(obj, writer, rendererMap, false);
        }

        /// <summary>
        /// Serialize any object into a string builder
        /// </summary>
        /// <param name="obj"></param>
		/// <param name="sb"></param>
		/// <param name="map">log4net renderer map</param>
		protected virtual void Serialize(object obj, TextWriter sb, RendererMap map, bool tryMapFirst)
        {
            var serialized = (tryMapFirst && SerializeObjectUsingRendererMap(obj, sb, map))
                    || SerializeNull(obj, sb) // null gate first, others do not expect nulls
                    || SerializeDictionary(obj as IDictionary, sb, map)
                    || SerializeString(obj as string, sb)
                    || SerializeChars(obj as char[], sb)
                    || SerializeBytes(obj as byte[], sb)
                    || SerializeDateTime(obj, sb)
                    || SerializeTimeSpan(obj, sb)
                    || SerializePrimitive(obj, sb)
                    || SerializeEnum(obj, sb)
                    || SerializeGuid(obj, sb)
                    || SerializeUri(obj as Uri, sb)
                    || SerializeArray(obj as IEnumerable, sb, map) // goes almost last not to interfere with string, char[], byte[]...
                    || SerializeObjectAsDictionary(obj, sb, map) // before last resort
                    ;

            if (!serialized)
                SerializeString(Convert.ToString(obj), sb); // last resort
        }

        /// <summary>
        /// Serialize null into a string builder
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="sb"></param>
		protected virtual bool SerializeNull(object obj, TextWriter sb)
        {
            if (obj != null && !DBNull.Value.Equals(obj)) return false;
            sb.Write("null");
            return true;
        }

        /// <summary>
        /// Serialize date and time into a string builder
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="sb"></param>
		protected virtual bool SerializeDateTime(object obj, TextWriter sb)
        {
            if (!(obj is DateTime)) return false;
            SerializeString(((DateTime)obj).ToString("o"), sb);
            return true;
        }

        /// <summary>
        /// Serialize time span into a string builder
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="sb"></param>
		protected virtual bool SerializeTimeSpan(object obj, TextWriter sb)
        {
            if (!(obj is TimeSpan)) return false;
            SerializePrimitive(((TimeSpan)obj).TotalSeconds, sb);
            return true;
        }

        /// <summary>
        /// Serialize int's, byte's, char's, bools and friends into a string builder
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="sb"></param>
		protected virtual bool SerializePrimitive(object obj, TextWriter sb)
        {
            if (obj == null) return false;

            var t = obj.GetType();

            switch (t.FullName)
            {
                case "System.Double":
                case "System.Float":
                    sb.WriteFormat("{0:r}", obj);
                    break;
                case "System.Char":
                    SerializeChars(new char[] { (char)obj }, sb);
                    break;
                case "System.Byte":
                    SerializeBytes(new byte[] { (byte)obj }, sb);
                    break;
                case "System.Decimal":
                    sb.Write(obj);
                    break;
                case "System.Boolean":
                    sb.Write(true.Equals(obj) ? "true" : "false");
                    break;
                default:
                    if (!t.GetTypeInfo().IsPrimitive)
                        return false;
                    else
                        sb.Write(obj);
                    break;
            }

            return true;
        }

        /// <summary>
        /// Serialize a dictionary into a string builder
        /// </summary>
        /// <param name="obj"></param>
		/// <param name="sb"></param>
		/// <param name="map">log4net renderer map</param>
		protected virtual bool SerializeDictionary(IDictionary obj, TextWriter sb, RendererMap map)
        {
            if (obj == null) return false;

            sb.Write("{");

            bool first = true;

            foreach (DictionaryEntry entry in (IDictionary)obj)
            {
                if (first)
                    first = false;
                else
                    sb.Write(",");

                sb.WriteFormat(@"""{0}"":", entry.Key);

                Serialize(entry.Value, sb, map, true);
            }

            sb.Write("}");

            return true;
        }

        /// <summary>
        /// Serialize enumerables into a string builder
        /// </summary>
        /// <param name="obj"></param>
		/// <param name="sb"></param>
		/// <param name="map">log4net renderer map</param>
		protected virtual bool SerializeArray(IEnumerable obj, TextWriter sb, RendererMap map)
        {
            if (obj == null) return false;

            sb.Write("[");

            bool first = true;

            foreach (var item in obj)
            {
                if (first)
                    first = false;
                else
                    sb.Write(",");

                Serialize(item, sb, map, true);
            }

            sb.Write("]");

            return true;
        }

        /// <summary>
        /// Serialize an object as a dictionary (last resort) into a string builder
        /// </summary>
        /// <param name="obj"></param>
		/// <param name="sb"></param>
		/// <param name="map">log4net renderer map</param>
		protected virtual bool SerializeObjectAsDictionary(Object obj, TextWriter sb, RendererMap map)
        {
            if (obj == null) return false;

            var dict = ObjToDict(obj, SaveType, SaveInternalType, TypeMemberName, Stringify, StringMemberName);
            SerializeDictionary(dict, sb, map);

            return true;
        }

        /// <summary>
        /// Serialize an object into a string builder using another serializer registered in the renderer map
        /// </summary>
        /// <param name="obj"></param>
		/// <param name="sb"></param>
		/// <param name="map">log4net renderer map</param>
		protected virtual bool SerializeObjectUsingRendererMap(Object obj, TextWriter sb, RendererMap map)
        {
            if (obj == null) return false;
            if (map == null) return false;

            var customSerializer = map.Get(obj) as IJsonRenderer;

            if (customSerializer == null) return false;

            customSerializer.RenderObject(map, obj, sb);

            return true;
        }

        /// <summary>
        /// Serialize escaped string into a string builder
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="sb"></param>
		protected virtual bool SerializeBytes(byte[] obj, TextWriter sb)
        {
            if (obj == null) return false;

            var str = Encoding.UTF8.GetString(obj);
            SerializeString(str, sb);

            return true;
        }

        /// <summary>
        /// Serialize escaped string into a string builder
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="sb"></param>
		protected virtual bool SerializeChars(char[] obj, TextWriter sb)
        {
            if (obj == null) return false;

            var str = new string(obj);
            SerializeString(str, sb);

            return true;
        }

        /// <summary>
        /// Serialize escaped string into a string builder
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="sb"></param>
		protected virtual bool SerializeString(object obj, TextWriter sb)
        {
            if (obj == null) return false;

            var str = Convert.ToString(obj);

            sb.Write(@"""");

            foreach (var c in str)
            {

                string cstring;

                if (EscapedChars.TryGetValue(c, out cstring))
                    sb.Write(cstring);
                else if (c < 32 || c > 126)
                    // c<32 nonprintable
                    // c=127 nonptintable
                    // c>127 encoding specific
                    sb.WriteFormat("\\u{0:X4}", (int)c);
                else
                    sb.Write(c);
            }

            sb.Write(@"""");

            return true;
        }

        /// <summary>
        /// Serialize URI into a string builder
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="sb"></param>
		protected virtual bool SerializeUri(Uri obj, TextWriter sb)
        {
            if (obj == null) return false;

            var str = Convert.ToString(obj);

            return SerializeString(str, sb);
        }

        /// <summary>
        /// Serialize enum into a string builder
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="sb"></param>
		protected virtual bool SerializeGuid(object obj, TextWriter sb)
        {
            if (obj == null || !(obj is Guid)) return false;

            var str = Convert.ToString(obj);

            return SerializeString(str, sb);
        }

        /// <summary>
        /// Serialize enum into a string builder
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="sb"></param>
		protected virtual bool SerializeEnum(object obj, TextWriter sb)
        {
            if (obj == null) return false;
            if (!obj.GetType().GetTypeInfo().IsEnum) return false;

            var str = Convert.ToString(obj);

            return SerializeString(str, sb);
        }

        /// <summary>
        /// Convert objects fields and props into a dictionary
        /// </summary>
        /// <param name="obj">object to be turned into a dictionary</param>
        /// <param name="saveType">preserve the type of the object? null => only when publicly visible</param>
        /// <param name="typeMemberName">where to preserve the type</param>
        /// <param name="stringify">call ToString() and save it</param>
        /// <param name="stringMemberName">where to preserve the string</param>
        /// <returns>dictionary of props and fields</returns>
        public static IDictionary ObjToDict(object obj, bool saveType, bool saveInternalType, string typeMemberName, bool stringify, string stringMemberName)
        {
            if (obj == null) return null;

            var flags = BindingFlags.Instance
                        | BindingFlags.Public
                        ;

            var type = obj.GetType();
            var props = type.GetProperties(flags);
            var flds = type.GetFields(flags);
            var dict = new Dictionary<string, object>(props.Length + flds.Length + 1);

            foreach (var fld in flds)
            {
                dict[fld.Name] = fld.GetValue(obj);
            }

            foreach (var prop in props)
            {
                dict[prop.Name] = prop.GetValue(obj, null);
            }

            if (saveType && (saveInternalType || type.GetTypeInfo().IsVisible))
                dict[typeMemberName] = type.FullName;

            if (stringify)
                dict[stringMemberName] = Convert.ToString(obj);

            return dict;
        }
    }
}
