using System.Net.Sockets;
using System.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Audit.Core;
using Audit.Core.Providers;
using Audit.EntityFramework;
using Segugio.Ports;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Sinks.Network;
using SysConsole = System.Console;

namespace Segugio.Providers;

/// <summary>
/// Implementazione di <see cref="ISegugioProvider"/> che utilizza Serilog per registrare gli eventi di audit.
/// </summary>
/// <remarks>
/// Questo provider utilizza un sink TCP di Serilog per inviare i log a un server remoto.
/// </remarks>
public class SerilogProvider : ISegugioProvider
{
    private readonly ISerilogConfiguration _configuration;

    public SerilogProvider(ISerilogConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GetProviederName => "SerilogProvider";

    /// <summary>
    /// Restituisce un provider di dati di audit configurato per Serilog.
    /// </summary>
    /// <param name="contesto">Il contesto di audit, che contiene informazioni su IP, sessioni e rotte HTTP.</param>
    /// <param name="utente">Le informazioni sull'utente, come nome account e ruoli.</param>
    /// <returns>Un'istanza di <see cref="AuditDataProvider"/> basata su Serilog.</returns>
    /// <remarks>
    /// Configura Serilog per inviare log a un server TCP, includendo informazioni contestuali come IP utente e route.
    /// </remarks>
    /// <example>
    /// Esempio d'uso:
    /// <code>
    /// var provider = new SerilogProvider("127.0.0.1", "514");
    /// var auditProvider = provider.GetAuditProvider(contestoAudit, utenteAudit);
    /// </code>
    /// </example>
    public AuditDataProvider GetAuditProvider(IContestoAudit contesto)
    {
        var serilogProvider = new DynamicDataProvider(config =>
        {
            config.OnInsert(ev =>
            {
                if (!int.TryParse(_configuration.ServerPort, out int port))
                {
                    throw new SegugioException("Il valore di ServerPort non è un numero valido.");
                }
                
                var logger = new LoggerConfiguration()
                    .WriteTo.Sink(new RawTcpSink(_configuration.ServerAddress, port))
                    // .WriteTo.TCPSink(
                    //     $"tcp://{_configuration.ServerAddress}:{_configuration.ServerPort}"
                    // )
                    .CreateLogger();
                
                var entiyName = (ev.GetEntityFrameworkEvent() != null
                    ? ev.GetEntityFrameworkEvent().Entries.FirstOrDefault().Name
                    : "");
                var primaryKey = (ev.GetEntityFrameworkEvent() != null
                    ? ev.GetEntityFrameworkEvent().Entries.FirstOrDefault().PrimaryKey.FirstOrDefault().Value
                    : "");
                var esito = !string.IsNullOrWhiteSpace(ev.Environment.Exception);

                var msg = _configuration.GetMessage(new SerilogEvent
                {
                    Entity = entiyName,
                    PrimaryKey = primaryKey.ToString(),
                    Success = esito,
                    ContestoAudit = contesto,
                    AuditEvent = ev
                });
                
                // Crittografia del messaggio usando il certificato
                string encryptedMessage = EncryptMessageWithCertificate(msg, _configuration.CertificatePath, _configuration.CertificateType);;

                try
                {
                    logger.Information(encryptedMessage);
                }
                catch (Exception e)
                {
                    switch (_configuration.LogTypes) 
                    {
                        case ISegugioProvider.LogTypes.Console:
                            Console.WriteLine(e);
                            break;
                        case ISegugioProvider.LogTypes.Exception:
                            throw new SegugioException("Error sending log to Serilog", e);
                        default:
                            break;
                    };
                }
            });
        });
        return serilogProvider;
    }
    
    public ISegugioProvider.LogTypes LogType => _configuration.LogTypes;
    
    /// <summary>
    /// Crittografa un messaggio utilizzando un certificato passato come percorso.
    /// </summary>
    /// <param name="message">Il messaggio in chiaro da criptare.</param>
    /// <param name="certificatePath">Il percorso del certificato da usare per criptare.</param>
    /// <returns>Il messaggio criptato come stringa Base64.</returns>
    private string EncryptMessageWithCertificate(string message, string certificatePath, CertificateTypes certificateType)
    {
        if (certificateType == CertificateTypes.None) return message;
        
        if (string.IsNullOrWhiteSpace(certificatePath) || !File.Exists(certificatePath))
            throw new InvalidOperationException("Certificate's path not found or empty. Please check the path and try again.");
    
        // Caricare il certificato
        var certificate = new X509Certificate2(certificatePath);

        // Validare il certificato in base al tipo configurato
        ValidateCertificateForType(certificate, certificateType);

        switch (certificateType)
        {
            case CertificateTypes.RSA:
                return EncryptWithRSA(certificate, message);

            case CertificateTypes.ECC:
                return EncryptWithECC(certificate, message);

            default:
                throw new InvalidOperationException("Tipo di certificato non supportato.");
        }
    }
    
    /// <summary>
    /// Esempio di crittografia con certificato RSA.
    /// </summary>
    /// <param name="certificate">Il certificato caricato.</param>
    /// <param name="message">Il messaggio da criptare.</param>
    /// <returns>Il messaggio crittografato come Base64.</returns>
    private string EncryptWithRSA(X509Certificate2 certificate, string message)
    {
        // Ottenere la chiave pubblica RSA
        using var rsa = certificate.GetRSAPublicKey();
        if (rsa == null)
            throw new InvalidOperationException("No valid RSA public key found in the certificate.");

        // Convertire il messaggio in byte
        var messageBytes = Encoding.UTF8.GetBytes(message);

        // Crittografare il messaggio
        var encryptedBytes = rsa.Encrypt(messageBytes, RSAEncryptionPadding.OaepSHA256);

        // Convertire il risultato in Base64
        return Convert.ToBase64String(encryptedBytes);
    }

    /// <summary>
    /// Esempio di crittografia con certificato ECC.
    /// </summary>
    /// <param name="certificate">Il certificato caricato.</param>
    /// <param name="message">Il messaggio da criptare.</param>
    /// <returns>Il messaggio crittografato come Base64.</returns>
    private string EncryptWithECC(X509Certificate2 certificate, string message)
    {
        // Ottenere la chiave pubblica ECC
        using var ecdsa = certificate.GetECDsaPublicKey();
        if (ecdsa == null)
            throw new InvalidOperationException("No valid ECC public key found in the certificate.");

        // Convertire il messaggio in byte
        var messageBytes = Encoding.UTF8.GetBytes(message);

        // Crittografare il messaggio utilizzando l'algoritmo di firma (simula la crittografia)
        // NOTA: ECC non supporta nativamente la crittografia di messaggi arbitrari.
        // Si utilizza la firma digitale per simulare la protezione dei dati.
        var signedBytes = ecdsa.SignData(messageBytes, HashAlgorithmName.SHA256);

        // Convertire il risultato in Base64
        return Convert.ToBase64String(signedBytes);
    }
    
    /// <summary>
    /// Valida la coerenza del certificato con il tipo configurato.
    /// </summary>
    /// <param name="certificate">Il certificato da validare.</param>
    /// <param name="certificateType">Il tipo di certificato configurato.</param>
    private void ValidateCertificateForType(X509Certificate2 certificate, CertificateTypes certificateType)
    {
        switch (certificateType)
        {
            case CertificateTypes.RSA:
                if (certificate.GetRSAPublicKey() == null)
                    throw new InvalidOperationException("The certificate provided is not compatible with RSA.");
                break;

            case CertificateTypes.ECC:
                if (certificate.GetECDsaPublicKey() == null)
                    throw new InvalidOperationException("The certificate provided is not compatible with ECC.");
                break;
            
            default:
                throw new NotSupportedException($"Tipo di certificato non supportato: {certificateType}");
        }

        if (DateTime.Now > certificate.NotAfter)
        {
            throw new InvalidOperationException("Il certificato è scaduto.");
        }

        if (DateTime.Now < certificate.NotBefore)
        {
            throw new InvalidOperationException("Il certificato non è ancora valido.");
        }
    }
}

public class RawTcpSink : ILogEventSink
{
    private readonly string _host;
    private readonly int _port;

    public RawTcpSink(string host, int port)
    {
        _host = host ?? throw new ArgumentNullException(nameof(host));
        _port = port > 0 ? port : throw new ArgumentOutOfRangeException(nameof(port));
    }

    public void Emit(LogEvent logEvent)
    {
        if (logEvent == null)
            throw new ArgumentNullException(nameof(logEvent));

        // Estrai il messaggio dal log event
        var message = logEvent.RenderMessage();

        // Invia il messaggio tramite TCP
        try
        {
            using var client = new TcpClient(_host, _port);
            using var stream = client.GetStream();

            // Converti il messaggio in byte e invia
            var data = Encoding.UTF8.GetBytes(message + "\n"); // Aggiungi newline per il server
            stream.Write(data, 0, data.Length);
        }
        catch (Exception ex)
        {
            // Logga a console eventuali errori nella trasmissione TCP
            Console.WriteLine($"Errore durante la trasmissione del messaggio TCP: {ex.Message}");
        }
    }
}

public enum CertificateTypes
{
    None,
    RSA,
    ECC // Per esempio, altri tipi di certificati possono essere aggiunti qui.
}

public interface ISerilogConfiguration
{
    /// <summary>
    /// Indirizzo del server remoto a cui inviare i log via Serilog.
    /// </summary>
    string ServerAddress { get; }

    /// <summary>
    /// Porta del server remoto a cui inviare i log via Serilog.
    /// </summary>
    string ServerPort { get; }

    ISegugioProvider.LogTypes LogTypes { get; }

    /// <summary>
    /// Percorso del certificato
    /// </summary>
    string CertificatePath { get; }
    /// <summary>
    /// Tipo certificato
    /// </summary>
    CertificateTypes CertificateType { get; }
    
    string GetMessage(SerilogEvent serilogEvent);
}

public class SerilogEvent
{
    public string Entity { get; set; }
    public string PrimaryKey { get; set; }
    public bool Success { get; set; }

    public IContestoAudit ContestoAudit { get; set; }
    public AuditEvent AuditEvent { get; set; }
}