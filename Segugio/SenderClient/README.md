# SenderClient

Progetto `SenderClient` che utilizza il sistema di audit `Segugio`, integrato con un database SQL Server per lo sviluppo e test di un sistema di Audit su differenti canali.

## Requisiti

- **.NET 8.0**
- **SQL Server**

## Configurazione del Database

Il progetto utilizza un database chiamato **Segugio**, che include due schemi principali:

1. **BusinessModel**: per gestire una entità per intercettarne le operazioni da tracciare, tabella `Persone`.
2. **Audit**: per registrare i log di audit in una tabella chiamata `EntityAuditLog`.

### Script di Creazione del Database

**Creazione del Database inizializzato**

```sql
-- Creazione del database Segugio
CREATE DATABASE Segugio;
GO

-- Utilizzare il database Segugio
USE Segugio;
GO

-- Creazione dello schema BusinessModel
CREATE SCHEMA BusinessModel;
GO

-- Creazione della tabella Persone nello schema BusinessModel
CREATE TABLE BusinessModel.Persone (
  Id BIGINT NOT NULL PRIMARY KEY,       -- Identificativo univoco
  Descrizione NVARCHAR(255) NOT NULL   -- Descrizione dell'entità
);
GO

-- Creazione dello schema Audit
CREATE SCHEMA Audit;
GO

-- Creazione della tabella EntityAuditLog nello schema Audit
CREATE TABLE Audit.EntityAuditLog (
  Id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,  -- Identificativo univoco del log
  UserName NVARCHAR(255) NULL,                  -- Nome utente
  UserRole NVARCHAR(255) NULL,                  -- Ruolo utente
  UserAdmin NVARCHAR(255) NULL,                 -- Account impersonato (amministratore)
  IpAddress NVARCHAR(50) NULL,                  -- Indirizzo IP dell'utente
  RouteData NVARCHAR(MAX) NULL,                 -- Informazioni di routing HTTP
  LastUpdate DATETIME NOT NULL,                 -- Timestamp dell'ultima modifica
  DataJSon NVARCHAR(MAX) NOT NULL,              -- Dati di audit formattati in JSON
  TimeStamp [timestamp] NULL,                   -- Timestamp dell'operazione  
);
GO
```

## Configurazione dell'Applicazione

### Connection String

Nel file `appsettings.json`, la stringa di connessione al database è configurata come segue:

```json
{
  "ConnectionStrings": {
    "SegugioConnection": "Server=./;Connection Timeout=30;persist security info=True;TrustServerCertificate=True;Integrated Security=SSPI;Initial Catalog=Segugio;"
  }
}
```

Assicurati di modificare questa stringa di connessione in base all'ambiente e alla configurazione specifica del tuo SQL Server.

### Configurazione del Servizio di Audit

Nel file `Program.cs`, il sistema di audit è configurato in questo modo, se vengono implementati nuovi provider possono essere aggiunti in questa configurazione per poterne fare un test:

```csharp
var segugioAuditor = builder.Services.BuildServiceProvider().GetRequiredService();
segugioAuditor.Setup(
    new List  
    {
      new SqlServerProvider(
        new AuditTableConfiguration(connectionString,"Audit","EntityAuditLog","UserName","DataJSon", 
            "LastUpdate", "UserRole", "UserAdmin", "Id", "IpAddress","RouteData"
        )
      ),
      new SerilogProvider("localhost", "514")
    }
);
```

## Esecuzione

1. Assicurati di aver creato il database e le tabelle eseguendo gli script SQL sopra riportati.
2. Configura il file `appsettings.json` per puntare al tuo database SQL Server.
3. Avvia l'applicazione con il comando seguente:
   ```bash
   dotnet run
   ```
4. Se l'applicazione è in fase di sviluppo, apri l'interfaccia Swagger per testare le API:
   ```
   http://localhost:<PORT>/swagger
   ```

## Note

- Il sistema di audit registra eventi nel database `Segugio` utilizzando i dettagli dell'utente e della richiesta HTTP.
- La configurazione prevede inoltre un provider Serilog configurato per inviare log via TCP (può essere personalizzato).
- A supporto del test per il provider di invio messaggi a server Syslog è presente il Progetto `SyslogServer` il quale deve essere avviato prima di avviare l'applicazione `SenderClient`.

## Collaboratori

- **Nome del team/progetto**: Segugio
- **Email supporto**: supporto@segugio.local
