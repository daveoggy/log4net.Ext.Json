using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net.ext.json.xunit.General;
using Xunit;
using Assert = NUnit.Framework.Assert;
using StringAssert = NUnit.Framework.StringAssert;
using log4net.Core;
using System.Collections;
using log4net.Util.Serializer;
using log4net.Layout;
using log4net.Layout.Pattern;
using log4net.ObjectRenderer;

namespace log4net.ext.json.xunit.Log
{
#if JsonBuiltinSerializer
    public class WithBuiltinSerializer1 : RepoTest
    {
        protected override string GetConfig()
        {
            return @"<log4net>
                        <root>
                          <level value='DEBUG'/>
                          <appender-ref ref='TestAppender'/>
                        </root>

                        <appender name='TestAppender' type='log4net.ext.json.xunit.General.TestAppender, log4net.ext.json.xunit'>
                          <layout type='log4net.Layout.SerializedLayout, log4net.Ext.Json'>
                             <serializingconverter type='log4net.Layout.Pattern.JsonPatternConverter, log4net.Ext.Json'>
                                <renderer type='log4net.ObjectRenderer.JsonObjectRenderer, log4net.Ext.Json'>
                                  <serializer type='log4net.Util.Serializer.JsonBuiltinSerializer, log4net.Ext.Json' />
                                </renderer>
                             </serializingconverter>
                             <decorator type='log4net.Layout.Decorators.StandardTypesDecorator, log4net.Ext.Json' />
                             <member value='message:messageobject' />
                          </layout>
                        </appender>
                      </log4net>";
        }

		protected override void RunTestLog(log4net.ILog log)
        {   
            var appenders = GetAppenders<TestAppender>(log.Logger);

            Assert.AreEqual(1, appenders.Length, "appenders Count");

            var tapp = appenders.Single();

            Assert.IsInstanceOf<SerializedLayout>(tapp.Layout, "layout type");
            var layout = ((SerializedLayout)tapp.Layout);
            Assert.IsInstanceOf<JsonPatternConverter>(layout.SerializingConverter, "converter type");
            var converter = ((JsonPatternConverter)layout.SerializingConverter);
            Assert.IsInstanceOf<JsonObjectRenderer>(converter.Renderer, "renderer type");
            var renderer = ((JsonObjectRenderer)converter.Renderer);
			Assert.IsInstanceOf<JsonBuiltinSerializer>(renderer.Serializer, "serializer type");

            log.Info(new { A = 1, B = new { X = "Y" } });

            var events = GetEventStrings(log.Logger);

            Assert.AreEqual(1, events.Length, "events Count");

            var le = events.Single();

            Assert.IsNotNull(le, "loggingevent");

            StringAssert.Contains(@"{""message"":{""A"":1,""B"":{""X"":""Y""}}}", le, "le has structured message");


        }
    }
#endif
}