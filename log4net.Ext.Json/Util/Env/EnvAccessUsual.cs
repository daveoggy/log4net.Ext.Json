using System;

namespace log4net.Ext.Json.Util.Env
{
#if !LimitedEnvAccess
    public class EnvAccessUsual : EnvAccessBasic
    {
        public override string GetMachineName() => Environment.MachineName;

        public override string GetCommandLine()=> Environment.CommandLine;

        public override string GetUserName() => Environment.UserName;

        public override string GetUserDomain() => Environment.UserDomainName;   

        public override long GetWorkingSet() => Environment.WorkingSet;   
    }
#endif
}