using Microsoft.AspNetCore.Routing;

namespace Segugio.Ports;

/// <summary>
/// Interfaccia per fornire informazioni contestuali per il sistema di audit.
/// </summary>
/// <remarks>
/// L'implementazione di questa interfaccia consente di recuperare dettagli sul contesto attuale,
/// come l'indirizzo IP remoto, l'ID sessione e i dati delle rotte HTTP.
/// </remarks>
public interface IContestoAudit
{
    /// <summary>
    /// Recupera l'indirizzo IP remoto dell'utente.
    /// </summary>
    /// <returns>Una stringa che rappresenta l'indirizzo IP remoto.</returns>
    string GetRemoteIpAddress();

    /// <summary>
    /// Recupera l'ID della sessione corrente.
    /// </summary>
    /// <returns>Una stringa che rappresenta l'ID della sessione.</returns>
    string GetSessionId();

    /// <summary>
    /// Recupera l'ID della terminale chiamante, se disponibile..
    /// </summary>
    /// <returns>Una stringa che rappresenta l'ID del terminale.</returns>
    string GetTerminalId();

    /// <summary>
    /// Recupera i dati relativi alla rotta HTTP attualmente utilizzata.
    /// </summary>
    /// <returns>
    /// Un oggetto <see cref="RouteData"/> che contiene informazioni sulla rotta HTTP.
    /// Può restituire <c>null</c> se nessuna rotta è disponibile.
    /// </returns>
    RouteData? GetHttpRouteData();
}