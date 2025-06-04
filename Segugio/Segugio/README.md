# Segugio

Segugio è una libreria per la gestione del sistema di audit in progetti .NET. Consente di configurare facilmente provider di audit e raccogliere informazioni di log dettagliate: indirizzi IP, sessioni, route HTTP, e molto altro. La libreria supporta un'architettura flessibile e altamente estendibile.

---

## Caratteristiche

- **Modulare**: Configura facilmente diversi provider di audit.
- **Flessibile**: Supporta audit per database, Serilog, file di log, ecc.
- **Integrabile**: Pensato per essere incluso in qualsiasi progetto .NET.
- **Personalizzabile**: Pienamente estendibile per adattarsi a qualsiasi esigenza di logging.

---

## Installazione

Puoi installare Segugio direttamente dal **Package Manager Console** usando il comando:

```bash
Install-Package Segugio
```

Oppure aggiungendolo nel file `.csproj`:

```xml
<PackageReference Include="Segugio" Version="1.0.0" />
```

## Configurazione

### Passaggi principali
1. **Implementa l'interfaccia `IContestoAudit`** per configurare informazioni contestuali dell'applicazione (indirizzo IP, ID sessione, dati di routing, ID utente, ruolo).
3. **Usa uno dei provider** già inclusi nella libreria.
4. **Crea uno o più provider** derivando dall'interfaccia `ISegugioProvider` per salvare i dati audit nel supporto desiderato.

---

## Esempio di utilizzo

### 1. Creazione del contesto di audit
```csharp
public class ContestoAudit : IContestoAudit
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    public ContestoApplicativo(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string GetCustomAttribute(string attributeName)
    {
        return attributeName switch
        {
            "IpAddress" => _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "",
            "RoutePath" => JsonSerializer.Serialize(_httpContextAccessor.HttpContext?.GetRouteData()??new RouteData()),
            "QueryPath" => $"{_httpContextAccessor.HttpContext?.GetRouteData().Values["controller"]}/{_httpContextAccessor.HttpContext?.GetRouteData().Values["action"]}",
            _ => ""
        };
    }
}
```

### 2. Uso di un provider già incluso nella libreria
Ad esempio, puoi configurare i due già previsti così:
```csharp
    new SqlServerProvider(
        new AuditTableConfiguration(connectionString,"AuditTableSchema","AuditTableName","IdField","EntityDataJSonField","LastUpdateField",
            new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("IpAddressField", "IpAddress"),
                new KeyValuePair<string, string>("RouteDataField", "RoutePath"),
                new KeyValuePair<string, string>("UserNameField", "UserName"),
                new KeyValuePair<string, string>("UserRoleField", "Role")
            },
            ISegugioProvider.LogTypes.Console
        )
    ),
    new SerilogProvider(new QradarConfiguration("serverSerilog", "port", ISegugioProvider.LogTypes.Console))
```
Puoi decidere inoltre il comportamento che deve avere il sistema di Audit per supportarti nello sviluppo impostando per ogni provider la modalità di log e gestione eventuali anomalie:
- **ISegugioProvider.LogTypes.None**: opzione di default, non scrive niente in console ed in caso di mancata registrazione sul provider di Audit non solleva eccezioni.
- **ISegugioProvider.LogTypes.Console**: Scrive in console messaggi di successo per registrazione dell'audit da parte del provider ed in caso di mancata registrazione scrive il problema in console senza sollevare eccezioni.
- **ISegugioProvider.LogTypes.Exception**: Non scrive nessun messaggio in console e nel caso di mancata registrazione dell'audit solleva un'eccezione di tipo SegugioException.


### 3. Creazione di un nuovo provider
Ad esempio, creare un nuovo provider per Serilog così:
```csharp
public class SerilogProvider : ISegugioProvider
{
    public string ServerAddress { get; set; } = "127.0.0.1";
    public string ServerPort { get; set; } = "514";

    public AuditDataProvider GetAuditProvider(IContestoAudit contesto, IUtenteAudit utente)
    {
        var serilogProvider = new DynamicDataProvider(config =>
        {
            config.OnInsert(ev =>
            {
                var logger = new LoggerConfiguration()
                    .WriteTo.TCPSink($"tcp://{ServerAddress}:{ServerPort}")
                    .CreateLogger();

                logger.Information($"Log {ev.EventType}");
            });
        });
        return serilogProvider;
    }
    
    public ISegugioProvider.LogTypes LogType => ISegugioProvider.LogTypes.None;
}
```

### 4. Setup del sistema di audit
Connetti tutto alla classe `SegugioAuditor` e configura il sistema:
```csharp
builder.Services.AddScoped<IContestoAudit, ContestoApplicativo>();
builder.Services.AddScoped<ISegugioAuditor, SegugioAuditor>();

var segugioAuditor = builder.Services.BuildServiceProvider().GetRequiredService<ISegugioAuditor>();
segugioAuditor.Setup(new List<ISegugioProvider>
{
    new SqlServerProvider(
        new AuditTableConfiguration(connectionString,"Audit","EntityAuditLog","Id","DataJSon","LastUpdate")
    ),
    new SerilogProvider(new QradarConfiguration("localhost", "514")),
    new CustomProvider()
});

// Sistema audit configurato!
```

### 5. Creazione dei log di audit
Ora puoi utilizzare il sistema di audit per registrare le attività:
```csharp
using AuditScope = Audit.Core.AuditScope;

public void OperazioneEsempio()
{
    using (var scope = AuditScope.Create("Operazione", () => new { Data = "Esempio" }, new { Utente = "Admin" }))
    {
        // Logica della tua operazione
    }
}
```

---

## Estendibilità

- **Aggiungere nuovi provider**: Implementa l'interfaccia `ISegugioProvider` per supportare altri mezzi di logging (es. MongoDB, ElasticSearch).
- **Contesto personalizzato**: Adatta la tua implementazione di `IContestoAudit` e `IUtenteAudit` per aggiungere altre informazioni come dati del server o informazioni client-specifiche.

---
