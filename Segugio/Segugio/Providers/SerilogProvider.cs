using Audit.Core;
using Audit.Core.Providers;
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
                var logger = new LoggerConfiguration()
                    .WriteTo.TCPSink($"tcp://{ServerAddress}:{ServerPort}")
                    .CreateLogger();

                logger.Information($"Log {ev.EventType}");
            });
        });
        return serilogProvider;
    }
}