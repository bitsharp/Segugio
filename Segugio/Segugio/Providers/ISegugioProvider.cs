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
    public enum LogTypes
    {
        None,
        Console,
        Exception
    }
    
    string GetProviederName { get; }
    /// <summary>
    /// Restituisce un provider di dati di audit configurato con il contesto e le informazioni dell'utente.
    /// </summary>
    /// <param name="contesto">Il contesto dell'applicativo con tutte le informazioni necessarie all'audit.</param>
    /// <returns>Un'istanza di <see cref="AuditDataProvider"/> configurata.</returns>
    AuditDataProvider GetAuditProvider(IContestoAudit contesto);
    
    LogTypes LogType { get; }
}