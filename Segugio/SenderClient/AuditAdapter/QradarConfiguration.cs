using Segugio.Providers;

namespace SenderClient.AuditAdapter;

public class QradarConfiguration : ISerilogConfiguration
{
    public QradarConfiguration(string serverAddress, string serverPort, CertificateTypes certificateType = CertificateTypes.None, ISegugioProvider.LogTypes logType = ISegugioProvider.LogTypes.None, string certificatePath = "")
    {
        ServerAddress = serverAddress;
        ServerPort = serverPort;
        CertificateType = certificateType;
        LogTypes = logType;
        if (!string.IsNullOrEmpty(certificatePath))
            CertificatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, certificatePath);
    }

    public string ServerAddress { get; }
    public string ServerPort { get; }
    public ISegugioProvider.LogTypes LogTypes { get; }
    public string CertificatePath { get; }
    public CertificateTypes CertificateType { get; }

    public string GetMessage(SerilogEvent serilogEvent)
    {
        var msg =
            $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss,fff")} " +
            $"user=\"{serilogEvent.ContestoAudit.GetCustomAttribute("UserName")}\" " +
            $"role=\"{serilogEvent.ContestoAudit.GetCustomAttribute("Role")}\" " +
            $"class=\"{serilogEvent.AuditEvent.Environment.CallingMethodName}\" " +
            $"ip=\"{serilogEvent.ContestoAudit.GetCustomAttribute("IpAddress")}\" " +
            $"query=\"{serilogEvent.ContestoAudit.GetCustomAttribute("QueryPath")}\" " +
            $"objectType=\"{serilogEvent.Entity}\" " +
            $"objectId=\"{serilogEvent.PrimaryKey}\" ";
        Console.WriteLine(msg);        
        return msg;
    }
}