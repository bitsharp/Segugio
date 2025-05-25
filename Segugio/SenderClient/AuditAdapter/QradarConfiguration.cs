using Segugio.Providers;

namespace SenderClient.AuditAdapter;

public class QradarConfiguration : ISerilogConfiguration
{
    public QradarConfiguration(string serverAddress, string serverPort, ISegugioProvider.LogTypes logType = ISegugioProvider.LogTypes.None)
    {
        ServerAddress = serverAddress;
        ServerPort = serverPort;
        LogTypes = logType;
    }

    public string ServerAddress { get; }
    public string ServerPort { get; }
    public ISegugioProvider.LogTypes LogTypes { get; }

    public string GetMessage(SerilogEvent serilogEvent)
    {
        return $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss,fff")} " +
               $"user=\"{serilogEvent.ContestoAudit.GetCustomAttribute("UserName")}\" " +
               $"role=\"{serilogEvent.ContestoAudit.GetCustomAttribute("Role")}\n " +
               $"class=\n{serilogEvent.AuditEvent.Environment.CallingMethodName}\n " +
               $"ip=\n{serilogEvent.ContestoAudit.GetCustomAttribute("IpAddress")}\n " +
               $"query=\n{serilogEvent.ContestoAudit.GetCustomAttribute("QueryPath")}\n " +
               $"objectType=\n{serilogEvent.Entity}\n " +
               $"objectId=\n{serilogEvent.PrimaryKey}\n ";
    }
}