using System;
using log4net.Util.Stamps;

namespace playground
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

			var s = Stamp.GetProcessId();

			Type.GetType("log4net.Layout.SerializedLayout, log4net.Ext.Json",true);
        }
    }
}
