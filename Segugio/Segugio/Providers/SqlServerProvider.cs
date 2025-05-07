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
/// Questo provider salva gli eventi di audit in una tabella configurata nel database SQL Server.
/// Include colonne personalizzate per registrare informazioni aggiuntive.
/// </remarks>
public class SqlServerProvider : ISegugioProvider
{
    private readonly AuditTableConfiguration _configuration;

    /// <summary>
    /// Costruisce un'istanza del provider SQL Server.
    /// </summary>
    /// <param name="connectionString">La stringa di connessione al database.</param>
    /// <param name="schemaName">Il nome dello schema della tabella di audit.</param>
    /// <param name="tableName">Il nome della tabella di audit.</param>
    /// <param name="fieldKeyName">Il nome della colonna chiave primaria.</param>
    /// <param name="dataColumnName">Il nome della colonna in cui salvare i dati JSON.</param>

    public SqlServerProvider(AuditTableConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Restituisce un provider di dati di audit configurato per SQL Server.
    /// </summary>
    /// <param name="contesto">Il contesto di audit, che fornisce informazioni di rete e sessione.</param>
    /// <param name="utente">Le informazioni sull'utente, inclusi account e ruoli.</param>
    /// <returns>Un'istanza di <see cref="AuditDataProvider"/> basata su SQL Server.</returns>
    /// <remarks>
    /// Salva gli eventi di audit nel database SQL Server. Sono supportate colonne personalizzate per informazioni aggiuntive come IP, route, e dettagli utente.
    /// </remarks>
    /// <example>
    /// Esempio d'uso:
    /// <code>
    /// var provider = new SqlServerProvider(
    ///     "Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=myPassword;",
    ///     "dbo",
    ///     "AuditLogs",
    ///     "Id",
    ///     "Data");
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
            customColumnList.Add(new CustomColumn(_configuration.UserName, ev => utente.GetNetworkAccount()));
        if (!string.IsNullOrEmpty(_configuration.UserRole))
            customColumnList.Add(new CustomColumn(_configuration.UserRole, ev => utente.GetRoles()));
        if (!string.IsNullOrEmpty(_configuration.UserAdmin))
            customColumnList.Add(new CustomColumn(_configuration.UserAdmin, ev => utente.GetImpersonatedAccount()));
        
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

public class AuditTableConfiguration
{
    public string ConnectionString { get; }
    public string SchemaName { get; }
    public string TableName { get; }
    public string FieldKeyName { get; }
    public string DataColumnName { get; }

    // Parametri string per colonne personalizzate
    public string LastUpdate { get; }
    public string IpAddress { get; }
    public string RouteData { get; }
    public string UserName { get; }
    public string UserRole { get; }
    public string UserAdmin { get; }

    public AuditTableConfiguration(
        string connectionString,
        string schemaName,
        string tableName,
        string userName,
        string dataColumnName,
        string lastUpdate,
        // optional parameter
        string userRole = "",
        string userAdmin = "",
        string fieldKeyName = "",
        string ipAddress = "",
        string routeData = ""
        )
    {
        ConnectionString = connectionString;
        SchemaName = schemaName;
        TableName = tableName;
        FieldKeyName = fieldKeyName;
        DataColumnName = dataColumnName;

        // Inizializza i parametri per colonne personalizzate dalle stringhe passate
        LastUpdate = lastUpdate;
        IpAddress = ipAddress;
        RouteData = routeData;
        UserName = userName;
        UserRole = userRole;
        UserAdmin = userAdmin;
    }
}
