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
using log4net.Core;
using log4net.Layout.Members;
using System.Collections;

namespace log4net.Layout
{
    /// <summary>
    /// This <see cref="IRawLayout"/> facilitates arranged members retrieval 
    /// in the form of a <see cref="Dictionary&lt;String,Object>"/>.
    /// </summary>
    /// <remarks>
    /// This is meant to be used as a <see cref="Layout.Pattern.JsonPatternConverter.Fetcher"/>
    /// </remarks>
    /// <author>Robert Sevcik</author>
    public class RawFlatArrangedLayout : RawArrangedLayout
    {
        /// <summary>
        /// Gather the <see cref="Members"/> in a FLAT dictionary. See <see cref="FlattenDictionary"/> for detail on flattening.
        /// </summary>
        /// <param name="loggingEvent"></param>
        /// <returns>a flat dictionary of members</returns>
        public override object Format(LoggingEvent loggingEvent)
        {
            var result = base.Format(loggingEvent);

            var dict = result as IDictionary;

            if (dict != null)
            {
                var flatdict = new Dictionary<object, object>();
                FlattenDictionary(flatdict, dict);
                result = flatdict;
            }

            return result;
        }

        /// <summary>
        /// Copy a recursively nested dictionary into a flat dictionary.
        /// </summary>
        /// <param name="flatdict">target flat dictionary</param>
        /// <param name="dict">source hierarchical dictionary</param>
        /// <param name="path">nesting path</param>
        /// <remarks>
        /// 
        /// * member keys/names are stringified
        /// * nested <see cref="IDictionary"/> member values are flattened with a "parent.child" notation
        /// * non-primitive values are stringified
        /// 
        /// </remarks>
        public static void FlattenDictionary(IDictionary flatdict, IDictionary dict, string path = null)
        {
            if (flatdict == null) throw new ArgumentNullException("flatdict");
            if (dict == null) throw new ArgumentNullException("dict");

            foreach (DictionaryEntry entry in dict)
            {
                var name = path == null
                            ? Convert.ToString(entry.Key)
                            : String.Format("{0}.{1}", path, entry.Key)
                            ;

                if (entry.Value is IDictionary)
                {
                    FlattenDictionary(flatdict, (IDictionary)entry.Value, name);
                }
                else if (entry.Value == null || entry.Value is string || entry.Value.GetType().IsPrimitive)
                {
                    flatdict[name] = entry.Value;
                }
                else
                {
                    flatdict[name] = Convert.ToString(entry.Value);
                }
            }
        }
    }
}
