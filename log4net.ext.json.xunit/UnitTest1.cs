using System;
using Xunit;

namespace log4net.Ext.Json.Xunit
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
			var t = Type.GetType("log4net.Layout.SerializedLayout");
        }
    }
}
