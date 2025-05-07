using Audit.Core;
using Audit.Core.ConfigurationApi;
using Segugio.Ports;
using Segugio.Providers;

namespace Segugio;

/// <summary>
/// Interfaccia per la gestione del sistema di audit nel contesto di Segugio.
/// </summary>
public interface ISegugioAuditor
{
    /// <summary>
    /// Configura il sistema di audit utilizzando uno o più provider specificati.
    /// </summary>
    /// <param name="providers">Elenco dei provider da utilizzare per l'audit.</param>
    void Setup(IList<ISegugioProvider> providers);
}

/// <summary>
/// Classe principale per la configurazione e gestione del sistema di audit Segugio.
/// </summary>
/// <remarks>
/// Questa classe consente di configurare e abilitare il sistema di audit basato su uno o più provider.
/// Utilizzala per centralizzare la gestione dei log di audit in modo modulare e flessibile.
/// </remarks>
public class SegugioAuditor : ISegugioAuditor
{
    private readonly IContestoAudit _contestoAudit;
    private readonly IUtenteAudit _utenteAudit;

    /// <summary>
    /// Costruisce un'istanza della classe <see cref="SegugioAuditor"/>.
    /// </summary>
    /// <param name="contestoAudit">Il contesto di audit per la configurazione (es., indirizzo IP, sessione).</param>
    /// <param name="utenteAudit">Le informazioni sull'utente per l'audit (es., ID utente, ruolo).</param>
    public SegugioAuditor(IContestoAudit contestoAudit, IUtenteAudit utenteAudit)
    {
        _contestoAudit = contestoAudit;
        _utenteAudit = utenteAudit;
    }

    /// <summary>
    /// Configura il sistema di audit utilizzando un elenco di provider.
    /// </summary>
    /// <param name="providers">
    /// Elenco di provider che implementano <see cref="ISegugioProvider"/>.
    /// I provider configurati vengono utilizzati per registrare gli eventi di audit.
    /// </param>
    /// <remarks>
    /// Questa configurazione abilita il sistema di audit per gestire eventi dagli ambienti desiderati.
    /// Ogni provider è responsabile del proprio backend (es., database, file di log, Serilog).
    /// </remarks>
    /// <example>
    /// Esempio di configurazione del sistema di audit:
    /// <code>
    /// var contesto = new ContestoAudit();
    /// var utente = new UtenteAudit();
    /// 
    /// var providers = new List&lt;ISegugioProvider&gt;
    /// {
    ///     new SerilogProvider("127.0.0.1", "514")
    /// };
    /// 
    /// var auditor = new SegugioAuditor(contesto, utente);
    /// auditor.Setup(providers);
    /// </code>
    /// </example>
    public void Setup(IList<ISegugioProvider> providers)
    {
        var compositeDataProvider = new CompositeDataProvider(
            providers
                .Select(p => p.GetAuditProvider(_contestoAudit, _utenteAudit))
                .ToList());
        Configuration.Setup().UseCustomProvider(compositeDataProvider);
    }
}