using Audit.Core;
using Segugio.Ports;

namespace Segugio.Providers;

/// <summary>
/// Interfaccia per i provider di audit utilizzati da Segugio.
/// </summary>
/// <remarks>
/// Implementa questa interfaccia per creare un provider personalizzato che gestisca la scrittura di eventi di audit verso il backend desiderato (es. database, Serilog, altri sistemi di log).
/// </remarks>
public interface ISegugioProvider
{
    /// <summary>
    /// Restituisce un provider di dati di audit configurato con il contesto e le informazioni dell'utente.
/// </summary>
/// <param name="contesto">Il contesto di audit, che fornisce informazioni di rete, sessione e route.</param>
/// <param name="utente">Le informazioni sull'utente, inclusi nome, ruolo e account impersonato.</param>
/// <returns>Un'istanza di <see cref="AuditDataProvider"/> configurata.</returns>
    AuditDataProvider GetAuditProvider(IContestoAudit contesto);
}