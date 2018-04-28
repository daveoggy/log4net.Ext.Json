using System;
using System.IO;

namespace log4net.Util
{
    public static class TextWriterExt
    {
        public static void WriteFormat(this TextWriter w, string format, params object[] values)
        {
			w.Write(string.Format(format, values));
        }
    }
}
