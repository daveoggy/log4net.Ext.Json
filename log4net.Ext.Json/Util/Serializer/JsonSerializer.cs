using System;

namespace log4net.Util.Serializer
{
#if JsonBuiltinSerializer
    public class JsonSerializer : JsonBuiltinSerializer { }
#else
	public class JsonSerializer : JsonHomebrewSerializer { }
#endif
}

