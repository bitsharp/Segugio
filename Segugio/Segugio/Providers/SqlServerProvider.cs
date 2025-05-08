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

    /// <summary>
    /// Restituisce un provider di dati di audit configurato per SQL Server.
    /// </summary>
    /// <param name="contesto">Il contesto di audit che include informazioni di rete e sessione.</param>
    /// <param name="utente">Le informazioni sull'utente che includono account e ruoli correnti.</param>
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
    ///     userName: "AuditUser",
    ///     dataColumnName: "AuditData",
    ///     lastUpdate: "LastUpdated"
    /// );
    ///
    /// var provider = new SqlServerProvider(configuration);
    /// var auditProvider = provider.GetAuditProvider(contestoAudit, utenteAudit);
    /// </code>
    /// </example>
    public AuditDataProvider GetAuditProvider(IContestoAudit contesto, IUtenteAudit utente)
    {
        var customColumnList = new List<CustomColumn>();
        if (!string.IsNullOrEmpty(_configuration.LastUpdate))
            customColumnList.Add(new CustomColumn(_configuration.LastUpdate, ev => DateTime.Now));
        if (!string.IsNullOrEmpty(_configuration.IpAddress))
            customColumnList.Add(new CustomColumn(_configuration.IpAddress, ev => contesto.GetRemoteIpAddress()));
        if (!string.IsNullOrEmpty(_configuration.RouteData))
            customColumnList.Add(new CustomColumn(_configuration.RouteData, ev => JsonSerializer.Serialize(contesto.GetHttpRouteData())));
        if (!string.IsNullOrEmpty(_configuration.UserName))
            customColumnList.Add(new CustomColumn(_configuration.UserName, ev => utente.GetUserAccount()));
        if (!string.IsNullOrEmpty(_configuration.UserRole))
            customColumnList.Add(new CustomColumn(_configuration.UserRole, ev => utente.GetRoles()));
        if (!string.IsNullOrEmpty(_configuration.UserAdmin))
            customColumnList.Add(new CustomColumn(_configuration.UserAdmin, ev => utente.GetRealAccount()));

        var sqlProvider = new SqlDataProvider()
        {
            ConnectionString = _configuration.ConnectionString,
            Schema = _configuration.SchemaName,
            TableName = _configuration.TableName,
            IdColumnName = _configuration.FieldKeyName,
            JsonColumnName = _configuration.DataColumnName,
            CustomColumns = customColumnList
        };
        return sqlProvider;
    }
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
    public string FieldKeyName { get; }
    /// <summary>Il nome della colonna che contiene i dati JSON dell'evento di audit.</summary>
    public string DataColumnName { get; }

    // Colonne personalizzate
    /// <summary>Il nome della colonna che registra l'ultima modifica.</summary>
    public string LastUpdate { get; }
    /// <summary>Il nome della colonna che registra l'indirizzo IP dell'utente.</summary>
    public string IpAddress { get; }
    /// <summary>Il nome della colonna che registra i dati delle route HTTP.</summary>
    public string RouteData { get; }
    /// <summary>Il nome della colonna che registra il nome account dell'utente.</summary>
    public string UserName { get; }
    /// <summary>Il nome della colonna che registra i ruoli dell'utente.</summary>
    public string UserRole { get; }
    /// <summary>Il nome della colonna che registra l'account di amministrazione.</summary>
    public string UserAdmin { get; }

    /// <summary>
    /// Inizializza un'istanza di <see cref="AuditTableConfiguration"/>.
    /// </summary>
    /// <param name="connectionString">Stringa di connessione al database SQL Server.</param>
    /// <param name="schemaName">Nome dello schema della tabella.</param>
    /// <param name="tableName">Nome della tabella di audit.</param>
    /// <param name="userName">Colonna per registrare il nome dell'utente.</param>
    /// <param name="dataColumnName">Colonna per contenere i dati di audit in formato JSON.</param>
    /// <param name="lastUpdate">Colonna per l'ultima modifica (valore temporale).</param>
    /// <param name="userRole">Colonna opzionale per i ruoli dell'utente.</param>
    /// <param name="userAdmin">Colonna opzionale per l'account amministrativo.</param>
    /// <param name="fieldKeyName">Colonna opzionale per la chiave primaria.</param>
    /// <param name="ipAddress">Colonna opzionale per l'indirizzo IP dell'utente.</param>
    /// <param name="routeData">Colonna opzionale per i dati della route.</param>
    public AuditTableConfiguration(
        string connectionString,
        string schemaName,
        string tableName,
        string userName,
        string dataColumnName,
        string lastUpdate,
        string userRole = "",
        string userAdmin = "",
        string fieldKeyName = "",
        string ipAddress = "",
        string routeData = "")
    {
        ConnectionString = connectionString;
        SchemaName = schemaName;
        TableName = tableName;
        FieldKeyName = fieldKeyName;
        DataColumnName = dataColumnName;

        // Inizializza i parametri per colonne personalizzate
        LastUpdate = lastUpdate;
        IpAddress = ipAddress;
        RouteData = routeData;
        UserName = userName;
        UserRole = userRole;
        UserAdmin = userAdmin;
    }
}