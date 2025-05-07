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
    /// <summary>
    /// Stringa di connessione al database SQL Server.
    /// </summary>
    public string ConnectionString { get; set; }

    /// <summary>
    /// Nome dello schema della tabella di audit.
    /// </summary>
    public string SchemaName { get; set; }

    /// <summary>
    /// Nome della tabella di audit.
    /// </summary>
    public string TableName { get; set; }

    /// <summary>
    /// Nome della colonna chiave primaria della tabella di audit.
    /// </summary>
    public string FieldKeyName { get; set; }

    /// <summary>
    /// Nome della colonna in cui verranno salvati i dati JSON degli eventi di audit.
    /// </summary>
    public string DataColumnName { get; set; }

    /// <summary>
    /// Costruisce un'istanza del provider SQL Server.
    /// </summary>
    /// <param name="connectionString">La stringa di connessione al database.</param>
    /// <param name="schemaName">Il nome dello schema della tabella di audit.</param>
    /// <param name="tableName">Il nome della tabella di audit.</param>
    /// <param name="fieldKeyName">Il nome della colonna chiave primaria.</param>
    /// <param name="dataColumnName">Il nome della colonna in cui salvare i dati JSON.</param>
    public SqlServerProvider(string connectionString, string schemaName, string tableName, string fieldKeyName, string dataColumnName)
    {
        ConnectionString = connectionString;
        SchemaName = schemaName;
        TableName = tableName;
        FieldKeyName = fieldKeyName;
        DataColumnName = dataColumnName;
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
        var sqlProvider = new SqlDataProvider()
        {
            ConnectionString = ConnectionString,
            Schema = SchemaName,
            TableName = TableName,
            IdColumnName = FieldKeyName,
            JsonColumnName = DataColumnName,
            CustomColumns = new List<CustomColumn>()
            {
                new CustomColumn("LastUpdate", ev => DateTime.Now),
                new CustomColumn("IpAddress", ev => contesto.GetRemoteIpAddress()),
                new CustomColumn("RouteData", ev => JsonSerializer.Serialize(contesto.GetHttpRouteData())),
                new CustomColumn("UserName", ev => utente.GetNetworkAccount()),
                new CustomColumn("UserRole", ev => utente.GetRoles()),
                new CustomColumn("UserAdmin", ev => utente.GetImpersonatedAccount())
            }
        };
        return sqlProvider;
    }
}