using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace log4net.Ext.Json.Xunit.General
{
    public class TestAppender : log4net.Appender.IAppender
    {
        public readonly IList<log4net.Core.LoggingEvent> Events = new List<log4net.Core.LoggingEvent>();
        public readonly IList<String> EventStrings = new List<String>();
        public string Name { get; set; }
        public log4net.Layout.ILayout Layout { get; set; }

        public void Close()
        {
            Events.Clear();
            EventStrings.Clear();
        }

        public void DoAppend(Core.LoggingEvent loggingEvent)
        {
            Events.Add(loggingEvent);

            var layout = Layout;
            var writer = new StringWriter();
            if (layout != null)
            {
                layout.Format(writer, loggingEvent);
            }
            else
            {
                loggingEvent.WriteRenderedMessage(writer);
            }

            EventStrings.Add(writer.ToString());
        }
    }
}
