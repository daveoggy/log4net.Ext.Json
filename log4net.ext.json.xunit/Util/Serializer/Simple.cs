using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Assert = NUnit.Framework.Assert;
using StringAssert = NUnit.Framework.StringAssert;

namespace log4net.ext.json.xunit.Util.Serializer
{
	public class Simple
	{
#if JsonBuiltinSerializer
		log4net.Util.Serializer.ISerializer SerializerBuiltIn;
#endif
		log4net.Util.Serializer.ISerializer SerializerHomeMade;

		public Simple()
		{

#if JsonBuiltinSerializer
			SerializerBuiltIn = new log4net.Util.Serializer.JsonBuiltinSerializer ();
#endif
			SerializerHomeMade = new log4net.Util.Serializer.JsonHomebrewSerializer();
		}

		[Theory]
		[InlineData(true, "true", "true")]
		[InlineData(false, "false", "false")]
		[InlineData(0, "0", "0")]
		[InlineData(0L, "0", "0")]
		[InlineData(0D, "0", "0")]
		[InlineData(int.MinValue, "-2147483648", "-2147483648")]
		[InlineData(int.MaxValue, "2147483647", "2147483647")]
		[InlineData(double.MinValue, "-1.7976931348623157E+308", "-1.7976931348623157E+308")]
		[InlineData(double.MaxValue, "1.7976931348623157E+308", "1.7976931348623157E+308")]

		[InlineData('*', @"""*""", @"""*""")]
		[InlineData((byte)168, @"168", @"""\uFFFD""")]

		[InlineData("xxx &;", @"""xxx &;""", @"""xxx \u0026;""")]
		[InlineData("ěščřžýáíé■", // builtin leaves unicode chars as is, homemade escapes them producing ASCII
			@"""ěščřžýáíé■""",
			@"""\u011B\u0161\u010D\u0159\u017E\u00FD\u00E1\u00ED\u00E9\u25A0""")]
		[InlineData("\"\\\b\f\n\r\t", @"""\""\\\b\f\n\r\t""", @"""\""\\\b\f\n\r\t""")]
		[InlineData("</>", @"""\u003c/\u003e""", @"""\u003c/\u003e""")]
		public void Serialize(object value, object expectedBuiltin, object expectedHomeMade)
		{

#if JsonBuiltinSerializer
			var resultBuiltIn = SerializerBuiltIn.Serialize (value, null);
			Assert.AreEqual (expectedBuiltin, resultBuiltIn, String.Format ("BuiltIn serialized {0}", value));
#endif
			var resultHomeMade = SerializerHomeMade.Serialize(value, null);
			Assert.AreEqual(expectedHomeMade, resultHomeMade, String.Format("HomeMade serialized {0}", value));
		}

		[Fact]
		public void SerializeNullableNull()
		{
			Serialize(new int?(), "null", "null");
		}

		[Fact]
		public void SerializeNullableInt()
		{
			Serialize(new int?(int.MinValue), "-2147483648", "-2147483648");
		}

		[Fact]
		public void SerializeNullableDouble()
		{
			Serialize(new double?(double.MinValue), "-1.7976931348623157E+308", "-1.7976931348623157E+308");
		}

		[Fact]
		public void SerializeDBNull()
		{
			Serialize(DBNull.Value, "null", "null");
		}

		[Fact]
		public void SerializeDecimal()
		{
			Serialize(decimal.MinValue, @"-79228162514264337593543950335", @"-79228162514264337593543950335");
		}

		[Fact]
		public void SerializeEnumFlags()
		{
			// builtin works enums into numbers, homemade into names
			Serialize(System.Text.RegularExpressions.RegexOptions.Compiled | System.Text.RegularExpressions.RegexOptions.Multiline,
				@"10",
				@"""Multiline, Compiled""");
		}

		[Fact]
		public void SerializeEnumSingle()
		{
			// builtin works enums into numbers, homemade into names
			Serialize(System.Text.RegularExpressions.RegexOptions.Compiled,
				@"8",
				@"""Compiled""");
		}

		[Fact]
		public void SerializeGuid()
		{
			Serialize(Guid.Empty, @"""00000000-0000-0000-0000-000000000000""", @"""00000000-0000-0000-0000-000000000000""");
		}

		[Fact]
		public void SerializeUri()
		{
			Serialize(new Uri("irc://xxx/yyy"), @"""irc://xxx/yyy""", @"""irc://xxx/yyy""");
		}

		/// <summary>
		/// We differ here on purpose?
		/// </summary>
		[Fact]
		public void SerializeBytes()
		{
			Serialize(new byte[] { 65, 255, 0, 128 }, @"[65,255,0,128]", @"""A\uFFFD\u0000\uFFFD""");
		}

		/// <summary>
		/// We differ here on purpose?
		/// </summary>
		[Fact]
		public void SerializeChars()
		{
			Serialize(new char[] { 'A', '¿', char.MinValue, '├' }, @"[""A"",""¿"",null,""├""]", @"""A\u00BF\u0000\u251C""");
		}

		/// <summary>
		/// We differ here on purpose with ISO standard date? Greater precision and better support.
		/// </summary>
		[Fact]
		public void SerializeDateTime()
		{
			Serialize(DateTime.Parse("2014-01-01 00:00:01"), @"""\/Date(1388534401000)\/""", @"""2014-01-01T00:00:01.0000000""");
		}

		/// <summary>
		/// We differ here on purpose?
		/// </summary>
		[Fact]
		public void SerializeTimeSpan()
		{
			// builtin handles TimeSpan as any object serializing it's fields and props, homemeade ony does seconds
			Serialize(TimeSpan.Parse("3.00:00:01.1234567"),
				@"{""Days"":3,""Hours"":0,""Milliseconds"":123,""Minutes"":0,""Seconds"":1,""Ticks"":2592011234567,""TotalDays"":3.0000130029710648,""TotalHours"":72.000312071305558,""TotalMilliseconds"":259201123.4567,""TotalMinutes"":4320.0187242783331,""TotalSeconds"":259201.1234567}",
				@"259201.1234567");
		}

		[Fact]
		public void SerializeObject()
		{
			Serialize(new object(), @"{}", @"{}");
		}

		[Fact]
		public void SerializeCustomPrivateObject()
		{
			Serialize(new CustomPrivateObject(), @"{""X"":""Y""}", @"{""X"":""Y""}");
		}

		[Fact]
		public void SerializeCustomPublicObject()
		{
			Serialize(new CustomPublicObject(), @"{""X"":""Y""}", @"{""X"":""Y""}");
		}

		[Fact]
		public void SerializeAnonymous()
		{
			Serialize(new { PROP = 1 }, @"{""PROP"":1}", @"{""PROP"":1}");
		}

		public class CustomPublicObject
		{
			public string X = "Y";
			protected string Z = "Y";
		}

		private class CustomPrivateObject : CustomPublicObject
		{

		}
	}
}
