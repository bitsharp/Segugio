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

### 1. Aggiungere la libreria via NuGet
Puoi installare Segugio direttamente dal **Package Manager Console** usando il comando:

```bash
Install-Package Segugio
```

Oppure aggiungendolo nel file `.csproj`:

```xml
<PackageReference Include="Segugio" Version="1.0.0" />
```

### 2. Usare la DLL
Se preferisci utilizzare il file DLL precompilato:
1. Copia il file `Segugio.dll` nella directory desiderata del tuo progetto.
2. Aggiungi un riferimento alla **DLL**:
    - In Visual Studio, fai clic con il tasto destro sul tuo progetto > **Add Reference** > **Browse** > seleziona il file `Segugio.dll`.

---

## Configurazione

### Passaggi principali
1. **Implementa l'interfaccia `IContestoAudit`** per configurare informazioni contestuali (indirizzo IP, ID sessione, dati di routing).
2. **Implementa l'interfaccia `IUtenteAudit`** per fornire informazioni sull'utente (es. ID utente, ruolo).
3. **Usa uno dei provider** già inclusi nella libreria.
4. **Crea uno o più provider** derivando dall'interfaccia `ISegugioProvider` per salvare i dati audit nel supporto desiderato.

---

## Esempio di utilizzo

### 1. Creazione del contesto di audit
```csharp
public class ContestoAudit : IContestoAudit
{
    public string GetRemoteIpAddress() => "192.168.1.1"; // Esempio di indirizzo IP remoto

    public string GetSessionId() => Guid.NewGuid().ToString(); // Genera un ID univoco per la sessione

    public string GetTerminalId() => Environment.MachineName; // Restituisce il nome del terminale

    public RouteData? GetHttpRouteData() => return new RouteData(); // Restituisci configurazione per la rotta HTTP (esempio semplificato)
}
```

### 2. Uso di un provider già incluso nella libreria
Ad esempio, puoi configurare i due già previsti così:
```csharp
    new SqlServerProvider(
        new AuditTableConfiguration(connectionString,"SchemaTabellaDiAudit","NomeTabellaDiAudit",
            "CampoUserName","CampoDatiJSon", "CampoUltimoAggiornamento", 
            "CampoProfilo", "CampoUtenteAmministratore", 
            "CampoIdTabellaAudit", "CampoIndirizzoIp","CampoRouteDataJson"
        )
    ),
    new SerilogProvider("serverSerilog", "porta")
```

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
}
```

### 4. Setup del sistema di audit
Connetti tutto alla classe `SegugioAuditor` e configura il sistema:
```csharp
var contestoAudit = new ContestoAudit();
var utenteAudit = new UtenteAudit();

var providers = new List<ISegugioProvider>
{
    new SerilogProvider("127.0.0.1", "514") // Configura il provider
};

var auditor = new SegugioAuditor(contestoAudit, utenteAudit);
auditor.Setup(providers);

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
