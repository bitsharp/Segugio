using Audit.Core;
using Audit.Core.ConfigurationApi;
using Segugio.Ports;
using Segugio.Providers;

namespace Segugio;

/// <summary>
/// Interfaccia per la gestione del sistema di audit all'interno del contesto dell'applicazione Segugio.
/// </summary>
/// <remarks>
/// L'interfaccia ISegugioAuditor fornisce un contratto per configurare il sistema di audit attraverso uno o più provider.
/// Implementazioni specifiche possono essere utilizzate per centralizzare e gestire eventi di audit.
/// </remarks>
public interface ISegugioAuditor
{
    /// <summary>
    /// Configura il sistema di audit utilizzando uno o più provider specificati.
    /// </summary>
    /// <param name="providers">Elenco dei provider da utilizzare per la gestione degli eventi di audit.</param>
    /// <remarks>
    /// Ogni provider nell'elenco viene configurato per contribuire alla gestione del registro di audit.
    /// Questa configurazione è progettata per supportare diversi backend (ad esempio, database, file di log o strumenti esterni come Serilog).
    /// </remarks>
    void Setup(IList<ISegugioProvider> providers);

    public AuditScope CreateScope(string eventType);
    public AuditScope CreateScope(string eventType, object target, object extraField);
}

/// <summary>
/// Classe dedicata alla configurazione e alla gestione centralizzata del sistema di audit dell'applicazione Segugio.
/// </summary>
/// <remarks>
/// La classe SegugioAuditor permette di configurare e abilitare il sistema di audit basato su provider specifici.
/// Utilizza contesto e informazioni sull'utente per una gestione modulare e flessibile degli eventi.
/// </remarks>
public class SegugioAuditor : ISegugioAuditor
{
    private readonly IContestoAudit _contestoAudit;

    /// <summary>
    /// Inizializza un'istanza della classe <see cref="SegugioAuditor"/>.
    /// </summary>
    /// <param name="contestoAudit">Istanza che rappresenta il contesto di audit (ad esempio, indirizzo IP, sessione corrente).</param>
    /// <param name="utenteAudit">Informazioni sull'utente utilizzate per l'audit (ad esempio, ID, ruolo).</param>
    /// <remarks>
    /// L'istanza viene utilizzata per registrare eventi di audit dettagliati basati sul contesto e sulle informazioni dell'utente.
    /// </remarks>
    public SegugioAuditor(IContestoAudit contestoAudit)
    {
        _contestoAudit = contestoAudit;
    }

    /// <summary>
    /// Configura il sistema di audit integrando uno o più provider specificati.
    /// </summary>
    /// <param name="providers">
    /// Elenco di provider che implementano <see cref="ISegugioProvider"/>. Ogni provider viene configurato
    /// per registrare gli eventi di audit utilizzando un backend specifico (ad esempio, database, servizi remoti o file).
    /// </param>
    /// <remarks>
    /// I provider abilitati tramite questa configurazione consentono una gestione ottimale e centralizzata degli eventi di audit.
    /// La modularità permette di aggiungere facilmente nuovi provider o sostituirne uno esistente per supportare esigenze specifiche.
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
                .Select(p => p.GetAuditProvider(_contestoAudit))
                .ToList());
        Configuration.Setup().UseCustomProvider(compositeDataProvider);
    }

    public AuditScope CreateScope(string eventType)
    {
        // return AuditScope.Create("Login", () => new { Data = "Esempio" }, new { Utente = "Admin" });
        return AuditScope.Create("Login", () => null);
    }

    public AuditScope CreateScope(string eventType, object target, object extraField)
    {
        return AuditScope.Create(eventType, () => target, extraField);
    }
}