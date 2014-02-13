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
using System.IO;
using log4net.Core;
using log4net.Layout.Arrangements;
using log4net.Layout.Members;
using log4net.ObjectRenderer;
using log4net.Util;
using log4net.Util.TypeConverters;
using log4net.Util.Serializer;

namespace log4net.Layout.Pattern
{
    /// <summary>
    /// Render the <see cref="Member"/>s using <see cref="Renderer"/> or using <see cref="RendererMap.FindAndRender(object, TextWriter)" />.
    /// Log4net properties can set (likely through <see cref="ConverterInfo"/>) to provide custom 
    /// <see cref="Renderer"/>, <see cref="Fetcher"/> and to pass <see cref="IArrangement" /> 
    /// and <see cref="ConverterInfo"/>s from hosting layout.
    /// Option can be set to introduce an additional <see cref="IArrangement" />
    /// Use it in a custom <see cref="PatternLayout.ConversionPattern" /> like this: "%serialize{DEFAULT;PID:processid}"
    /// </summary>
    /// <author>Robert Sevcik</author>
    public class JsonPatternConverter : PatternLayoutConverter, IObjectRenderer, IOptionHandler
    {
        #region Properties

        /// <summary>
        /// How to render the members is decided here. By default it is a <see cref="JsonObjectRenderer.Default"/>
        /// </summary>
        public IObjectRenderer Renderer { get; set; }

        /// <summary>
        /// What to render is decided here. By default it is a <see cref="RawArrangedLayout"/> 
        /// and it's members can be arranged - see <see cref="Prepare"/>
        /// </summary>
        public IRawLayout Fetcher { get; set; }

        /// <summary>
        /// If the default <see cref="Fetcher"/> (<see cref="RawArrangedLayout"/>) is used, these are it's Members.
        /// These members can be arranged - see <see cref="Prepare"/>
        /// </summary>
        public IList<IMember> Members { get; private set; }

        #endregion

        /// <summary>
        /// Create instance with a default <see cref="Fetcher" /> and <see cref="Renderer" />
        /// </summary>
        public JsonPatternConverter()
        {
            var dictLayout = new RawArrangedLayout();
            this.Fetcher = dictLayout;
            this.Members = dictLayout.Members;

            this.Renderer = JsonObjectRenderer.Default;

            // we're not going to bother, user decides where the exception will go.
            this.IgnoresException = false;
        }

        #region PatternLayoutConverter override implementation

        /// <summary>
        /// Render an object which will most likely be a <see cref="LoggingEvent"/>
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="state"></param>
        protected override void Convert(TextWriter writer, object state)
        {
            if (state != null && state is LoggingEvent)
            {
                Convert(writer, (LoggingEvent)state);
            }
            else
            {
                RenderObject(null, state, writer);
            }
        }

        /// <summary>
        /// Render what comes from the  <see cref="Fetcher" /> using  <see cref="Renderer" /> or default renderer.
        /// </summary>
        /// <param name="writer"><see cref="TextWriter" /> that will receive the formatted result.</param>
        /// <param name="loggingEvent">The <see cref="LoggingEvent" /> on which the pattern converter should be executed.</param>
        protected override void Convert(TextWriter writer, LoggingEvent loggingEvent)
        {
            var obj = Fetcher.Format(loggingEvent);
            var map = loggingEvent.Repository.RendererMap;
            RenderObject(map, obj, writer);
        }

        #endregion

        #region IObjectRenderer implementation

        /// <summary>
        /// Render <paramref name="obj"/> into the <paramref name="writer"/>
        /// </summary>
        /// <param name="map">from <see cref="LoggingEvent.Repository"/></param>
        /// <param name="obj">value to be rendered</param>
        /// <param name="writer">writer to write obj to</param>
        public void RenderObject(RendererMap map, object obj, TextWriter writer)
        {
            var renderer = Renderer 
                ?? (map == null ? null : map.Get(obj)) 
                ?? JsonObjectRenderer.Default ?? map.DefaultRenderer;

            renderer.RenderObject(map, obj, writer);
        }

        #endregion

        #region IOptionHandler implementation

        /// <summary>
        /// Activate the options that were previously set with calls to properties.
        /// <see cref="Renderer"/> of type <see cref="IObjectRenderer"/> is taken from Properties["renderer"] if missing
        /// <see cref="Fetcher"/> of type <see cref="IRawLayout"/> is taken from Properties["fetcher"] if missing
        /// <see cref="IArrangement"/> is taken from Properties["arrangement"] and from <i>option</i>.
        /// Converters to be used in arrangements are taken from Properties["converters"], an array of <see cref="ConverterInfo"/>
        /// Members are arranged using <see cref="Prepare"/>
        /// </summary>
        /// <remarks>
        /// <para>
        /// This allows an object to defer activation of its options until all
        /// options have been set. This is required for components which have
        /// related options that remain ambiguous until all are set.
        /// </para>
        /// <para>
        /// If a component implements this interface then this method must be called
        /// after its properties have been set before the component can be used.
        /// </para>
        /// </remarks>
        public virtual void ActivateOptions()
        {
            // this allows the serializer to be injected easily
            var factory = (ISerializerFactory)Properties["serializerfactory"];
            if (factory != null) Renderer = new JsonObjectRenderer() { Factory = factory };

            Renderer = (IObjectRenderer)Properties["renderer"] ?? Renderer;
            Fetcher = (IRawLayout)Properties["fetcher"] ?? Fetcher;
            var converters = ((ConverterInfo[])Properties["converters"]);
            var arrangement = (IArrangement)Properties["arrangement"];

            Prepare(Option, Members, converters, arrangement);
        }


        /// <summary>
        /// Prepare the Members using arrangements. See <see cref="ActivateOptions"/>
        /// </summary>
        /// <param name="option">Option to parse into an additional <see cref="IArrangement"/> using <see cref="ArrangementConverter.GetArrangement"/></param>
        /// <param name="members">Members to be arranged</param>
        /// <param name="converters">Converters used for arrangement</param>
        /// <param name="arrangement">Arrangement to organize the members</param>
        protected virtual void Prepare(string option, IList<IMember> members, ConverterInfo[] converters, IArrangement arrangement)
        {
            bool arranged = false;

            if (arrangement != null)
            {
                arrangement.SetConverters(converters);
                arrangement.Arrange(members);
                arranged = true;
            }

            if (!String.IsNullOrEmpty(option))
            {
                arrangement = ArrangementConverter.GetArrangement(option);
                if (arrangement != null)
                {
                    arrangement.SetConverters(converters);
                    arrangement.Arrange(members);
                    arranged = true;
                }
            }

            if (!arranged)
            {
                // cater for bare defaults
                arrangement = new DefaultArrangement();
                arrangement.SetConverters(converters);
                arrangement.Arrange(members);
            }
        }

        #endregion


    }
}
