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
    /// <summary>
    /// Recupera il nome dell'account utente corrente (quello di rete o dell'account impersonaificato, se presente).
    /// </summary>
    /// <returns>Una stringa che rappresenta il nome dell'account utente.</returns>
    string GetUserAccount();

    /// <summary>
    /// Recupera il nome dell'account utente collegato (quello di rete) se str impersonificando un altro utente.
    /// </summary>
    /// <returns>
    /// Una stringa che rappresenta l'account di rete collegato durante l'impersonazione di un altro utente.
    /// Se non ci sono account impersonificati, potrebbe restituire una stringa vuota.
    /// </returns>
    string GetRealAccount();

    /// <summary>
    /// Recupera i ruoli associati all'utente corrente.
    /// </summary>
    /// <returns>Una stringa che rappresenta i ruoli dell'utente corrente.</returns>
    string GetRoles();
}