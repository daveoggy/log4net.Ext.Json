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
using log4net.Layout.Arrangements;
using log4net.Layout.Members;
using log4net.Layout.Pattern;
using log4net.Util;
using log4net.Util.TypeConverters;

namespace log4net.Layout
{
    /// <summary>
    /// Enable an external serializer (JSON) to participate in PaternLayout 
    /// with variable member configuration using <see cref="IArrangement"/>s.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The goal of this class is to serialize a <see cref="log4net.Core.LoggingEvent"/> 
    /// as a string. The results depend on the <i>Members</i> organized by <see cref="IArrangement"/>s.
    /// </para>
    /// <para>
    /// Custom <i>renderer</i> and <i>fetcher</i> can be provided if the default 
    /// <see cref="JsonPatternConverter"/> is used or another implementation 
    /// follows this convention:
    /// log4net property: renderer, type <see cref="log4net.ObjectRenderer.IObjectRenderer" />
    /// log4net property: fetcher, type <see cref="log4net.Layout.IRawLayout" />
    /// </para>
    /// <para>
    /// Collected <i>arrangements</i> and <i>converters</i> are also passed as properties and used in 
    /// <see cref="JsonPatternConverter"/>:
    /// log4net property: arrangement, type <see cref="log4net.Layout.Arrangements.IArrangement" />
    /// log4net property: converters, type <see cref="log4net.Util.ConverterInfo" />[]
    /// </para>
    /// <para>
    /// This class is not concerned with how the data is rendered. It only provides a configuration shortcut
    /// to organize members into structures suitable for JSON serialization. Serialization is then performed
    /// by a PatternConverter, the <see cref="JsonPatternConverter"/> by default.
    /// </para>
    /// </remarks>
    /// <example>
    /// You can use a default configuration. Note that default default default is used when no other arrangements exist.
    /// <code>
    /// &lt;default /&gt; to use the default default members
    /// &lt;value value="nxlog" /&gt; to use the default members suitable for nxlog
    /// </code>
    /// </example>
    /// <example>
    /// You can use member configurations:
    /// <code>
    /// &lt;default /&gt; to use a default before any custom members.
    /// &lt;member value="nxlog" /&gt; to use the default members suitable for nxlog.
    /// &lt;member&gt; value="nxlog" /&gt; to use the default members suitable for nxlog.
    /// </code>
    /// </example>
    /// <example>
    /// You can use the pattern configuration to allow simple configurations of complex requirements:
    /// <code>
    /// &lt;arrangement value="MyOwnMember:appdomain" /&gt; to add a member with custom name.
    /// &lt;arrangement value="Day|It is %date{dddd} today" /&gt;. to render members using <see cref="PatternLayout"/>.
    /// &lt;arrangement value="Host=Name:hostname\;ProcessId\;Memory\;timestamp" /&gt;. to add nested members (note the \;).
    /// &lt;arrangement value="log4net.Layout.Arrangements.RemovalArrangement!" /&gt;. to add any custom arrangement.
    /// &lt;arrangement value="log4net.Layout.Arrangements.RemovalArrangement!Message" /&gt;. to add any custom arrangement with an option.
    /// &lt;arrangement value="Month%date:MMM" /&gt;. to run a <see cref="PatternLayout"/> converter with an option (useful more in conversionPattern).
    /// &lt;arrangement value="DEFAULT!nxlog" /&gt;. to add a default arrangement.
    /// &lt;arrangement value="CLEAR" /&gt;. to add remove all members.
    /// &lt;arrangement value="REMOVE!Source.*" /&gt;. to add remove specific members matching Regex pattern.
    /// </code>
    /// </example>
    /// <example>
    /// You can remove members from default:
    /// <code>
    /// &lt;default /&gt; to use a default before any custom members.
    /// &lt;remove value="Message" /&gt; to remove the Message member.
    /// &lt;arrangement value="Data:message" /&gt; to use reintroduce the message under different name.
    /// </code>
    /// </example>
    /// <example>
    /// You can also use the <see cref="PatternLayout.ConversionPattern"/> configurations:
    /// <code>
    /// &lt;conversionPattern&gt; value="DEFAULT!nxlog;UserName;HostName" /&gt; to use the default members suitable for nxlog, username and hostname.
    /// &lt;conversionPattern&gt; value="%d ... %serialize ..." /&gt; to use the <see cref="PatternLayout"/> style.
    /// </code>
    /// </example>
    /// <author>Robert Sevcik</author>
    public class SerializedLayout : PatternLayout
    {
        #region Static defaults and initialization

        /// <summary>
        /// This will be used as a default conversion pattern.
        /// Destination: <seealso cref="PatternLayout.ConversionPattern"/>
        /// </summary>
        public static string DefaultSerializePattern = "DEFAULT";

        /// <summary>
        /// This is the default serializing pattern converter name, 
        /// which should match (+%) with the pattern. 
        /// Destination: <seealso cref="SerializerName"/>
        /// </summary>
        public static string DefaultSerializerName = "serialize";

        /// <summary>
        /// The default type of serializing pattern converter. 
        /// Destination: <seealso cref="SerializerType"/>
        /// </summary>
        public static Type DefaultSerializerType = typeof(JsonPatternConverter);

        /// <summary>
        /// Static constructor to initialize the environment - <see cref="ArrangementConverter.Init"/>.
        /// </summary>
        static SerializedLayout()
        {
            ArrangementConverter.Init();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The type of serializing conversion pattern to use.
        /// </summary>
        public Type SerializerType { get; set; }

        /// <summary>
        /// The name to use for the serializing conversion pattern
        /// </summary>
        public String SerializerName { get; set; }

        /// <summary>
        /// The serializer used to <see cref="Format"/> the <see cref="LoggingEvent"/>
        /// </summary>
        public PatternConverter Serializer { get; set; }

        #endregion

        #region Internal fields

        /// <summary>
        /// Keep the collected arrangements here.
        /// </summary>
        private readonly MultipleArrangement m_arrangement = new MultipleArrangement();

        /// <summary>
        /// FIXME: Who know why the parrent class calls ActivateOptions() from constructor?
        /// It seems unnecessary and causes issues here. We use this field to 
        /// suspend the call to ActivateOptions() from constructor
        /// </summary>
        private bool m_constructed = false;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs an JsonLayout with empty <i>Members</i>, no <i>Style</i>, and default <i>serializer</i>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The default just produces an empty JSON object string.
        /// </para>
        /// <para>
        /// As per the <see cref="log4net.Core.IOptionHandler"/> contract the <see cref="ActivateOptions"/>
        /// method must be called after the properties on this object have been
        /// configured.
        /// </para>
        /// </remarks>
        public SerializedLayout()
            : base(DefaultSerializePattern)
        {
            // exception can be rendered so we do not ignore exceptions
            // note: when this was true (default) AppenderSkeleton.RenderLoggingEvent 
            // would add the exception, which is invalid in JSON context
            IgnoresException = false;

            SerializerType = DefaultSerializerType;
            SerializerName = DefaultSerializerName;

            // now we can allow ActivateOptions()
            m_constructed = true;
        }

        #endregion

        #region Implementation of IOptionHandler, override of PatternLayout

        /// <summary>
        /// Activate the options that were previously set with calls to properties.
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
        /// <para>
        /// The strange constructor call to this method is suspended using 
        /// <see cref="m_constructed"/>.
        /// </para>
        /// </remarks>
        public override void ActivateOptions()
        {
            if (!m_constructed) return;

            // pass control to parent in case we do not get a serializer :o[
            base.ActivateOptions();

            // just to get those converters
            var parser = CreatePatternParser(String.Empty);

            // Extract discovered converters
            var converters = Enumerable.ToArray(
                                Enumerable.Cast<ConverterInfo>(
                                    parser.PatternConverters.Values
                                )
                             );

            var arrangement = new MultipleArrangement();

            if (m_arrangement.Arrangements.Count != 0)
                arrangement.AddArrangement(m_arrangement);

            var patternArrangement = ArrangementConverter.GetArrangement(ConversionPattern);
            if (patternArrangement != null) arrangement.AddArrangement(patternArrangement);

            var name = SerializerName ?? DefaultSerializerName;
            var type = SerializerType ?? DefaultSerializerType;
            var info = (parser.PatternConverters.ContainsKey(name)
                            ? parser.PatternConverters[name] as ConverterInfo
                            : null
                        ) ?? CreateSerializerInfo(name, type);

            Serializer = CreateSerializer(info);

            if (Serializer != null)
                SetUpSerializer(Serializer, converters, arrangement);
        }

        #endregion

        #region Override of PatternLayout

        /// <summary>
        /// Produces a formatted string as specified by the Serializer.
        /// </summary>
        /// <param name="loggingEvent">the event being logged</param>
        /// <param name="writer">The TextWriter to write the formatted event to</param>
        /// <remarks>
        /// If Serializer is not set, we default to base implementation.
        /// </remarks>
        public override void Format(System.IO.TextWriter writer, LoggingEvent loggingEvent)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }
            if (loggingEvent == null)
            {
                throw new ArgumentNullException("loggingEvent");
            }

            if (Serializer == null)
                base.Format(writer, loggingEvent);
            else
                Serializer.Format(writer, loggingEvent);
        }

        #endregion

        #region Internal methods


        /// <summary>
        /// Fetch our own <see cref="PatternConverter"/> Serializer.
        /// </summary>
        /// <param name="info">description of the PatternConverter</param>
        /// <returns>pattern converter set up</returns>
        /// <remarks>
        /// <para>
        /// Please note that properties are only supported with log4net 1.2.11 and above.
        /// </para>
        /// </remarks>
        protected virtual PatternConverter CreateSerializer(ConverterInfo info)
        {
            var conv = info.Type == null ? null : Activator.CreateInstance(info.Type) as PatternConverter;
            if (conv == null) conv = new JsonPatternConverter();
            
#if !LOG4NET_1_2_10_COMPATIBLE
            conv.Properties = info.Properties;
#endif

            return conv;
        }

        /// <summary>
        /// Add <see cref="PatternConverter.Properties"/>, call <see cref="IOptionHandler.ActivateOptions"/> 
        /// </summary>
        /// <param name="conv">serializer to be set up</param>
        /// <param name="converters">converters to be used collected from parent class</param>
        /// <param name="arrangement">arrangement to be used collected from parent class</param>
        /// <remarks>
        /// <para>
        /// Please note that properties are only supported with log4net 1.2.11 and above.
        /// </para>
        /// </remarks>
        protected virtual void SetUpSerializer(PatternConverter conv, ConverterInfo[] converters, IArrangement arrangement)
        {
#if !LOG4NET_1_2_10_COMPATIBLE
            conv.Properties["arrangement"] = arrangement;
            conv.Properties["converters"] = converters;
#endif

            IOptionHandler optionHandler = conv as IOptionHandler;
            if (optionHandler != null)
            {
                optionHandler.ActivateOptions();
            }
        }

        /// <summary>
        /// Instantiate our own Serializer info
        /// </summary>
        /// <remarks>
        /// <see cref="SerializerName"/> and <see cref="SerializerType"/> properties
        /// </remarks>
        /// <returns>the info created</returns>
        /// <exception cref="InvalidOperationException">for invalid types see <see cref="PatternConverter"/> abstract class.</exception>
        protected virtual ConverterInfo CreateSerializerInfo(string name, Type type)
        {
            return new ConverterInfo() { Name = name, Type = type };
        }

        #endregion

        #region Configuration methods

        /// <summary>
        /// Add an arbitrary <see cref="IArrangement"/>. 
        /// This method will be most useful for XML configuration.
        /// </summary>
        /// <param name="value">the arrangement</param>
        public virtual void AddArrangement(IArrangement value)
        {
            m_arrangement.AddArrangement(value);
        }

        /// <summary>
        /// Add an <see cref="DefaultArrangement"/> that can be plain pattern string.
        /// This method will be most useful for XML configuration.
        /// </summary>
        /// <param name="value">the arrangement</param>
        public virtual void AddDefault(DefaultArrangement value)
        {
            m_arrangement.AddArrangement(value);
        }

        /// <summary>
        /// Add a single <see cref="Member"/> that can be plain pattern string. 
        /// Note that <see cref="Member"/> implements <see cref="IArrangement"/> as well.
        /// This method will be most useful for XML configuration.
        /// </summary>
        /// <param name="value">the member</param>
        public virtual void AddMember(IMember value)
        {
            m_arrangement.AddArrangement(value);
        }

        /// <summary>
        /// With <see cref="RemovalArrangement"/> remove all or 
        /// <seealso cref="System.Text.RegularExpressions.Regex"/> specific members.
        /// This method will be most useful for XML configuration.
        /// </summary>
        /// <param name="value">the removal</param>
        public virtual void AddRemove(RemovalArrangement value)
        {
            m_arrangement.AddArrangement(value);
        }

        /// <summary>
        /// Remove all members.
        /// This method will be most useful for XML configuration.
        /// </summary>
        public virtual void Clear()
        {
            m_arrangement.AddArrangement(new RemovalArrangement());
        }

        #endregion

    }
}
