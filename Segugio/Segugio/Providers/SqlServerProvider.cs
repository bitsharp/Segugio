using System.Text.Json;
using Audit.Core;
using Audit.SqlServer;
using Audit.SqlServer.Providers;
using Segugio.Ports;

namespace Segugio.Providers;

/// <summary>
/// Implementazione di <see cref="ISegugioProvider"/> che utilizza SQL Server per salvare gli eventi di audit.
/// </summary>
/// <remarks>
/// Questo provider inserisce gli eventi di audit in una tabella configurabile del database SQL Server.
/// È compatibile con colonne personalizzate per registrare informazioni aggiuntive come indirizzo IP, dati delle route HTTP, e dettagli utente.
/// Offre una soluzione scalabile per centralizzare i log e garantire l'integrità delle informazioni.
/// </remarks>
public class SqlServerProvider : ISegugioProvider
{
    private readonly AuditTableConfiguration _configuration;

    /// <summary>
    /// Inizializza un'istanza di <see cref="SqlServerProvider"/> con la configurazione specificata.
    /// </summary>
    /// <param name="configuration">
    /// Un oggetto <see cref="AuditTableConfiguration"/> che specifica i dettagli richiesti
    /// per la connessione al database, il nome della tabella e le colonne di audit.
    /// </param>
    /// <remarks>
    /// La configurazione include la stringa di connessione e i metadati richiesti per identificare la tabella
    /// di audit, consentendo personalizzazioni su misura per esigenze specifiche.
    /// </remarks>
    public SqlServerProvider(AuditTableConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GetProviederName => "SqlServerProvider";

    /// <summary>
    /// Restituisce un provider di dati di audit configurato per SQL Server.
    /// </summary>
    /// <param name="contesto">Il contesto di audit che fornisce al provider le informazioni di dell'applicativo che lo sta usando.</param>
    /// <returns>Un'istanza di <see cref="AuditDataProvider"/> configurata per il database SQL Server.</returns>
    /// <remarks>
    /// Configura un provider che invia gli eventi di audit al database, utilizzando colonne personalizzate per supportare
    /// la registrazione di informazioni avanzate come timestamp, IP dell'utente e altre informazioni contestuali.
    /// </remarks>
    /// <example>
    /// Esempio d'uso:
    /// <code>
    /// var configuration = new AuditTableConfiguration(
    ///     connectionString: "Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=myPassword;",
    ///     schemaName: "dbo",
    ///     tableName: "AuditLogs",
    ///     dataColumnName: "AuditData",
    ///     lastUpdate: "LastUpdated",
    ///     customColumns: new List&lt;KeyValuePair&lt;string, string&gt;&gt;( {"EntityName", "EntityNameContesto"}, {"...", "..."} )
    /// );
    ///
    /// var provider = new SqlServerProvider(configuration);
    /// var auditProvider = provider.GetAuditProvider(contestoAudit, utenteAudit);
    /// </code>
    /// </example>
    public AuditDataProvider GetAuditProvider(IContestoAudit contesto)
    {
        var customColumnList = new List<CustomColumn>();
        if (!string.IsNullOrEmpty(_configuration.LastUpdateField))
            customColumnList.Add(new CustomColumn(_configuration.LastUpdateField, ev => DateTime.Now));

        foreach (var customColumn in _configuration.CustomColumns)
            customColumnList.Add(new CustomColumn(customColumn.Key, ev => contesto.GetCustomAttribute(customColumn.Value)));
        
        var sqlProvider = new SqlDataProvider()
        {
            ConnectionString = _configuration.ConnectionString,
            Schema = _configuration.SchemaName,
            TableName = _configuration.TableName,
            IdColumnName = _configuration.KeyField,
            JsonColumnName = _configuration.JsonAuditField,
            CustomColumns = customColumnList
        };
        return sqlProvider;
    }

    public ISegugioProvider.LogTypes LogType => _configuration.LogType;
}

/// <summary>
/// Configurazione della tabella di audit per il provider SQL Server.
/// </summary>
/// <remarks>
/// Specifica i dettagli necessari per configurare e utilizzare una tabella di audit in SQL Server.
/// Include informazioni come connessione al database, schema, tabella e nomi delle colonne (standard e personalizzate).
/// </remarks>
public class AuditTableConfiguration
{
    /// <summary>La stringa di connessione al database SQL Server.</summary>
    public string ConnectionString { get; }
    /// <summary>Il nome dello schema in cui si trova la tabella di audit.</summary>
    public string SchemaName { get; }
    /// <summary>Il nome della tabella di audit in SQL Server.</summary>
    public string TableName { get; }
    /// <summary>Il nome della colonna chiave primaria.</summary>
    public string KeyField { get; }
    /// <summary>Il nome della colonna che contiene i dati JSON dell'evento di audit.</summary>
    public string JsonAuditField { get; }

    /// <summary>Il nome della colonna che registra l'ultima modifica.</summary>
    public string LastUpdateField { get; }
    
    // <summary> Elenco di definizioni di colonne personalizzate (Nome colonna, attributo Contesto.</summary>
    public List<KeyValuePair<string, string>> CustomColumns { get; }
    
    public ISegugioProvider.LogTypes LogType { get; }
    
    /// <summary>
    /// Inizializza un'istanza di <see cref="AuditTableConfiguration"/>.
    /// </summary>
    /// <param name="ConnectionString">Stringa di connessione al database SQL Server.</param>
    /// <param name="SchemaName">Nome dello schema della tabella.</param>
    /// <param name="TableName">Nome della tabella di audit.</param>
    /// <param name="KeyField">Colonna opzionale per la chiave primaria.</param>
    /// <param name="JsonAuditField">Colonna per contenere i dati di audit in formato JSON.</param>
    /// <param name="LastUpdateField">Colonna per l'ultima modifica (valore temporale).</param>
    public AuditTableConfiguration(
        string connectionString,
        string schemaName,
        string tableName,
        string keyField,
        string jsonAuditField,
        string lastUpdateField, 
        List<KeyValuePair<string, string>> customColumns = null,
        ISegugioProvider.LogTypes logType = ISegugioProvider.LogTypes.None)
    {
        ConnectionString = connectionString;
        SchemaName = schemaName;
        TableName = tableName;
        KeyField = keyField;
        JsonAuditField = jsonAuditField;
        LastUpdateField = lastUpdateField;
        CustomColumns = customColumns;
        LogType = logType;
    }
}