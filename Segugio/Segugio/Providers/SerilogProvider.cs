using System.Security;
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
    private readonly ISerilogConfiguration _configuration;

    public SerilogProvider(ISerilogConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GetProviederName => "SerilogProvider";

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
    public AuditDataProvider GetAuditProvider(IContestoAudit contesto)
    {
        var serilogProvider = new DynamicDataProvider(config =>
        {
            config.OnInsert(ev =>
            {
                var logger = new LoggerConfiguration()
                    .WriteTo
                    .TCPSink(
                        $"tcp://{_configuration.ServerAddress}:{_configuration.ServerPort}") // TCP sink configuration
                    .CreateLogger();

                var entiyName = (ev.GetEntityFrameworkEvent() != null
                    ? ev.GetEntityFrameworkEvent().Entries.FirstOrDefault().Name
                    : "");
                var primaryKey = (ev.GetEntityFrameworkEvent() != null
                    ? ev.GetEntityFrameworkEvent().Entries.FirstOrDefault().PrimaryKey.FirstOrDefault().Value
                    : "");
                var esito = !string.IsNullOrWhiteSpace(ev.Environment.Exception);

                var msg = _configuration.GetMessage(new SerilogEvent
                {
                    Entity = entiyName,
                    PrimaryKey = primaryKey.ToString(),
                    Success = esito,
                    ContestoAudit = contesto,
                    AuditEvent = ev
                });

                try
                {
                    logger.Information(msg);
                }
                catch (Exception e)
                {
                    switch (_configuration.LogTypes) 
                    {
                        case ISegugioProvider.LogTypes.Console:
                            Console.WriteLine(e);
                            break;
                        case ISegugioProvider.LogTypes.Exception:
                            throw new SegugioException("Error sending log to Serilog", e);
                        default:
                            break;
                    };
                }
            });
        });
        return serilogProvider;
    }
    
    public ISegugioProvider.LogTypes LogType => _configuration.LogTypes;
}

public interface ISerilogConfiguration
{
    /// <summary>
    /// Indirizzo del server remoto a cui inviare i log via Serilog.
    /// </summary>
    string ServerAddress { get; }

    /// <summary>
    /// Porta del server remoto a cui inviare i log via Serilog.
    /// </summary>
    string ServerPort { get; }

    ISegugioProvider.LogTypes LogTypes { get; }

    string GetMessage(SerilogEvent serilogEvent);
}

public class SerilogEvent
{
    public string Entity { get; set; }
    public string PrimaryKey { get; set; }
    public bool Success { get; set; }

    public IContestoAudit ContestoAudit { get; set; }
    public AuditEvent AuditEvent { get; set; }
}