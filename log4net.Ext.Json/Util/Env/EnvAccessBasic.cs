using System;
using System.Reflection;
using log4net.Util;

namespace log4net.Ext.Json.Util.Env
{
    /// <summary>
    /// Env acces that will work in netstandard1.x
    /// </summary>
    /// <remarks>
    /// Values are cached statically on first access
    /// </remarks>
    public class EnvAccessBasic : IEnvAccess
    {
        public virtual string GetMachineName() => HostNameAccess.Value;
        public virtual string GetCommandLine() => null;
        public virtual string GetUserName() => UserNameAccess.Value;
        public virtual string GetUserDomain() => UserDomainAccess.Value;
        public virtual int GetProcessId() => ProcessIdAccess.Value;
        public virtual string GetAppName() => WebAppNameAccess.Value == null ? AppNameAccess.Value : WebAppNameAccess.Value;
        public virtual string GetAppPath() => WebAppNameAccess.Value == null ? AppDirAccess.Value : AppNameAccess.Value;
        public virtual long GetWorkingSet() => 0;

        #region Private lazy static factories and caches
        private static class ProcessIdAccess
        {
            public static int Value { get; }
            static ProcessIdAccess()
            {
                Value = System.Diagnostics.Process.GetCurrentProcess().Id;
            }
        }
        private static class HostNameAccess
        {
            public static string Value { get; }
            static HostNameAccess()
            {
                Value =
                    Environment.GetEnvironmentVariable("CUMPUTERNAME")
                    ?? Environment.GetEnvironmentVariable("HOSTNAME")
                    ?? System.Net.Dns.GetHostName();
            }
        }
        private static class UserNameAccess
        {
            public static string Value { get; }
            static UserNameAccess()
            {
                Value =
                    Environment.GetEnvironmentVariable("USERNAME")
                    ?? Environment.GetEnvironmentVariable("USER");
            }
        }
        private static class UserDomainAccess
        {
            public static string Value { get; }
            static UserDomainAccess()
            {
                Value =
                    Environment.GetEnvironmentVariable("USERDOMAIN");
            }
        }
        private static class WebAppNameAccess
        {
            public static string Value { get; }
            static WebAppNameAccess()
            {
                // use reflection to avoid deps on web
                var hostingEnvType = Type.GetType("System.Web.Hosting.HostingEnvironment", false);
                if(hostingEnvType == null)
                    return;

                var t = hostingEnvType.GetTypeInfo();
                Value = t.GetDeclaredProperty("SiteName").GetValue(null) as string;
            }
        }
        private static class AppNameAccess
        {
            public static string Value { get; }
            static AppNameAccess()
            {
#if (NoAppDomain)
                Value = null;
#else
                Value = AppDomain.CurrentDomain.FriendlyName;
#endif
            }
        }
        private static class AppDirAccess
        {
            public static string Value { get; }
            static AppDirAccess()
            {
                Value = System.IO.Directory.GetCurrentDirectory();
                //Value = AppContext.BaseDirectory;
                //Value = AppDomain.CurrentDomain.BaseDirectory;
            }
        }
        #endregion
    }
}