using System;

namespace log4net.Util.Serializer
{

	#if !NONETJSON
	/// <summary>
	/// Json serializer based on framework builtin.
	/// </summary>
	public class JsonSerializer : JsonBuiltinSerializer	{	}
	#else
	/// <summary>
	/// Json serializer implemented naively in project - possibly slow.
	/// </summary>
	public class JsonSerializer : JsonHomebrewSerializer	{	}
	#endif
}

