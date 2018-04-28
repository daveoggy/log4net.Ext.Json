namespace log4net.Ext.Json.Util.Env
{
    public interface IEnvAccess
    {
        string GetMachineName();
        string GetCommandLine();
        string GetUserName();
        string GetUserDomain();
        string GetAppName();
        string GetAppPath();
        int GetProcessId();
        long GetWorkingSet();
    }
}