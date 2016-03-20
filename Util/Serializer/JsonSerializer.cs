using System;

namespace log4net.Util.Serializer
{

	#if FRAMEWORK_3_5_OR_ABOVE && !CLIENT_PROFILE && !NETCF
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

