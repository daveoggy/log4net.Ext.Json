using System;

namespace log4net.Ext.Json.Util.Env
{

#if LimitedEnvAccess
    public class EnvAccess : EnvAccessBasic
    {        
    }
#else
    public class EnvAccess : EnvAccessUsual
    {
    }
#endif
}