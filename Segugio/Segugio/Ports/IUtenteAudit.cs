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
    /// Recupera il nome dell'account di rete associato all'utente corrente.
    /// </summary>
    /// <returns>Una stringa che rappresenta il nome dell'account di rete.</returns>
    string GetNetworkAccount();

    /// <summary>
    /// Recupera il nome dell'account impersonato dall'utente, se presente.
    /// </summary>
    /// <returns>
    /// Una stringa che rappresenta l'account impersonato.
    /// Se non ci sono account impersonati, potrebbe restituire una stringa vuota.
    /// </returns>
    string GetImpersonatedAccount();

    /// <summary>
    /// Recupera i ruoli associati all'utente corrente.
    /// </summary>
    /// <returns>Una stringa che rappresenta i ruoli dell'utente corrente.</returns>
    string GetRoles();
}