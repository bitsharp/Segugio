using Audit.Core;
using Audit.Core.Providers;
using Audit.EntityFramework;
using Segugio.Ports;
using Serilog;
using Serilog.Sinks.Network;

namespace Segugio.Providers;

/// <summary>
/// Implementazione di <see cref="ISegugioProvider"/> che utilizza Serilog per registrare gli eventi di audit.
/// </summary>
/// <remarks>
/// Questo provider utilizza un sink TCP di Serilog per inviare i log a un server remoto.
/// </remarks>
public class SerilogProvider : ISegugioProvider
{
    /// <summary>
    /// Indirizzo del server remoto a cui inviare i log via Serilog.
    /// </summary>
    public string ServerAddress { get; set; }

    /// <summary>
    /// Porta del server remoto a cui inviare i log via Serilog.
    /// </summary>
    public string ServerPort { get; set; }

    /// <summary>
    /// Costruisce un'istanza del provider Serilog.
    /// </summary>
    /// <param name="serverAddress">L'indirizzo IP o il nome host del server di log.</param>
    /// <param name="serverPort">La porta TCP del server di log.</param>
    public SerilogProvider(string serverAddress, string serverPort)
    {
        ServerAddress = serverAddress;
        ServerPort = serverPort;
    }

    /// <summary>
    /// Restituisce un provider di dati di audit configurato per Serilog.
    /// </summary>
    /// <param name="contesto">Il contesto di audit, che contiene informazioni su IP, sessioni e rotte HTTP.</param>
    /// <param name="utente">Le informazioni sull'utente, come nome account e ruoli.</param>
    /// <returns>Un'istanza di <see cref="AuditDataProvider"/> basata su Serilog.</returns>
    /// <remarks>
    /// Configura Serilog per inviare log a un server TCP, includendo informazioni contestuali come IP utente e route.
    /// </remarks>
    /// <example>
    /// Esempio d'uso:
    /// <code>
    /// var provider = new SerilogProvider("127.0.0.1", "514");
    /// var auditProvider = provider.GetAuditProvider(contestoAudit, utenteAudit);
    /// </code>
    /// </example>
    public AuditDataProvider GetAuditProvider(IContestoAudit contesto, IUtenteAudit utente)
    {
        var serilogProvider = new DynamicDataProvider(config =>
        {
            config.OnInsert(ev =>
            {
                // Recupera l'oggetto target e l'azione
                var action = contesto.GetHttpRouteData().Values["action"];
                var doppioApice = '\"';
                
                var logger = new LoggerConfiguration()
                    .WriteTo.TCPSink($"tcp://{ServerAddress}:{ServerPort}") // TCP sink configuration
                    .CreateLogger();

                var msg = (ev.GetEntityFrameworkEvent() != null ? ev.GetEntityFrameworkEvent().Entries.FirstOrDefault().Name : "");
                var actionType = (ev.GetEntityFrameworkEvent() != null ? ev.GetEntityFrameworkEvent().Entries.FirstOrDefault().Name : "");
                var primaryKey = (ev.GetEntityFrameworkEvent() != null ? ev.GetEntityFrameworkEvent().Entries.FirstOrDefault().PrimaryKey.FirstOrDefault().Value : "");

                logger.Information($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss,fff")} " +
                                   $"user={doppioApice}{utente.GetUserAccount()}{doppioApice} " +
                                   $"sessionId={doppioApice}{contesto.GetSessionId()}{doppioApice} " +
                                   $"terminaleId={doppioApice}{contesto.GetTerminalId()}{doppioApice} " +
                                   $"class={doppioApice}{ev.Environment.CallingMethodName}{doppioApice} " +
                                   $"msg={doppioApice}{action} {msg}{doppioApice} " +
                                   $"ip={doppioApice}{contesto.GetRemoteIpAddress()}{doppioApice} " +
                                   $"query={doppioApice}/{contesto.GetHttpRouteData().Values["controller"]}/{action}{doppioApice} " +
                                   $"kpmgCode={doppioApice}KLOG1{ action switch
                                       {
                                           "Login" => "001",
                                           "Logout" => "002",
                                           "Insert" => "012",
                                           "Update" => "013",
                                           "Delete" => "014",
                                           _ => "011" }
                                   }0{doppioApice} " +
                                   $"objectType={doppioApice}{actionType}{doppioApice} " +
                                   $"objectId={doppioApice}{primaryKey}{doppioApice} ");
            });
        });
        return serilogProvider;
    }
}