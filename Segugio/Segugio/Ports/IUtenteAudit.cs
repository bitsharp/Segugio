namespace Segugio.Ports;

/// <summary>
/// Interfaccia per fornire informazioni sull'utente relative al sistema di audit.
/// </summary>
/// <remarks>
/// L'implementazione di questa interfaccia consente di recuperare dettagli sull'utente,
/// come il nome dell'account di rete, account impersonati e ruoli.
/// </remarks>
public interface IUtenteAudit
{
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