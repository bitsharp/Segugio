using Segugio.Providers;

namespace SenderClient.AuditAdapter;

public class QradarConfiguration : ISerilogConfiguration
{
    public QradarConfiguration(string serverAddress, string serverPort)
    {
        ServerAddress = serverAddress;
        ServerPort = serverPort;
    }

    public string ServerAddress { get; }
    public string ServerPort { get; }
    public string GetMessage(SerilogEvent serilogEvent)
    {
        var doppioApice = '\"';

        return $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss,fff")} " +
               $"user={doppioApice}{serilogEvent.ContestoAudit.GetUserAccount()}{doppioApice} " +
               $"sessionId={doppioApice}{serilogEvent.ContestoAudit.GetSessionId()}{doppioApice} " +
               $"terminaleId={doppioApice}{serilogEvent.ContestoAudit.GetTerminalId()}{doppioApice} " +
               $"class={doppioApice}{serilogEvent.AuditEvent.Environment.CallingMethodName}{doppioApice} " +
               $"msg={doppioApice}{serilogEvent.Action} {serilogEvent.Entity}{doppioApice} " +
               $"ip={doppioApice}{serilogEvent.ContestoAudit.GetRemoteIpAddress()}{doppioApice} " +
               $"query={doppioApice}/{serilogEvent.ContestoAudit.GetHttpRouteData().Values["controller"]}/{serilogEvent.Action}{doppioApice} " +
               $"kpmgCode={doppioApice}KLOG{(serilogEvent.Success ? "1" : "0")}{serilogEvent.Action switch
                   {
                       "Login" => "001",
                       "Logout" => "002",
                       "Insert" => "012",
                       "Update" => "013",
                       "Delete" => "014",
                       _ => "011" }
               }0{doppioApice} " +
               $"objectType={doppioApice}{serilogEvent.Entity}{doppioApice} " +
               $"objectId={doppioApice}{serilogEvent.PrimaryKey}{doppioApice} ";
    }
}