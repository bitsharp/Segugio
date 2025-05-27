using Microsoft.AspNetCore.Routing;

namespace Segugio.Ports;

/// <summary>
/// Interfaccia per fornire informazioni contestuali per il sistema di audit.
/// </summary>
/// <remarks>
/// L'implementazione di questa interfaccia consente di recuperare dettagli sul contesto applicativo attuale
/// </remarks>
public interface IContestoAudit
{
    /// <summary>
    /// Recupera un attributo del contesto applicativo personalizzato dall'utente.'
    /// </summary>
    /// <returns>Una stringa che rappresenta il valore richiesto.</returns>
    string GetCustomAttribute(string attributeName);
}