using System;
using System.Reflection;
namespace log4net.Util
{
	public static class Compatibility
	{
#if (Net20Reflection)
		public static Type GetTypeInfo(this Type t)
        {
			return t;
        }
		
		public static PropertyInfo GetDeclaredProperty(this Type t, string propertyName)
        {
		    return t.GetProperty(propertyName);
		}
		
		public static object GetValue(this PropertyInfo pi, object target)
        {
            return pi.GetValue(target, null);
        }
#endif
    }
}
