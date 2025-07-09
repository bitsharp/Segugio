using System.Net.Sockets;
using System.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Audit.Core;
using Audit.Core.Providers;
using Audit.EntityFramework;
using Microsoft.Extensions.Configuration;
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

        try
        {
            if (string.IsNullOrWhiteSpace(_configuration.ServerAddress))
                throw new SegugioException("L'indirizzo del server non può essere vuoto.");

            if (!int.TryParse(_configuration.ServerPort, out int port))
                throw new SegugioException("Il valore di ServerPort non è un numero valido.");
            if (port < 1 || port > 65535)
                throw new SegugioException("La porta del server deve essere un numero compreso tra 1 e 65535.");
        }
        catch (Exception e)
        {
            HandleException(e);
        }
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
                var logger = new LoggerConfiguration()
                    .WriteTo.Sink(new RawTcpSink(_configuration))
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

    private void HandleException(Exception e)
    {
        switch (_configuration.LogTypes) 
        {
            case ISegugioProvider.LogTypes.Console:
                Console.WriteLine(e);
                break;
            case ISegugioProvider.LogTypes.Exception:
                throw new SegugioException("Generic error in SerilogProvider", e);
        };
    }
    
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

        return certificateType switch
        {
            CertificateTypes.RSA_WithPrivateKey => EncryptWithRSAPrivateKey(certificate, message),
            CertificateTypes.RSA_WithPublicKey => EncryptWithRSAPublicKey(certificate, message),
            CertificateTypes.ECDsa_WithPrivateKey => EncryptWithECDsa(certificate, message, true),
            CertificateTypes.ECDsa_WithPublicKey => EncryptWithECDsa(certificate, message, false),
            _ => throw new InvalidOperationException($"Unsupported certificate type {certificateType.ToString()}. Please check the certificate type and try again.")
        };    
    }
    
    /// <summary>
    /// Esempio di crittografia con certificato RSA con chiave pubblica.
    /// </summary>
    /// <param name="certificate">Il certificato caricato.</param>
    /// <param name="message">Il messaggio da criptare.</param>
    /// <returns>Il messaggio crittografato come Base64.</returns>
    private string EncryptWithRSAPublicKey(X509Certificate2 certificate, string message)
    {
        if (certificate.HasPrivateKey)
            throw new InvalidOperationException("The certificate provided has a private key. Please use a certificate with a public key only.");
        
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
    /// Esempio di crittografia con certificato RSA con chiave privata.
    /// </summary>
    /// <param name="certificate">Il certificato caricato.</param>
    /// <param name="message">Il messaggio da criptare.</param>
    /// <returns>Il messaggio crittografato come Base64.</returns>
    private string EncryptWithRSAPrivateKey(X509Certificate2 certificate, string message)
    {
        if (!certificate.HasPrivateKey)
            throw new InvalidOperationException("The certificate provided hasn't a private key. Please use a certificate with a private key only.");
        
        // Ottenere la chiave private RSA
        using var rsa = certificate.GetRSAPrivateKey();
        if (rsa == null)
            throw new InvalidOperationException("No valid RSA private key found in the certificate.");

        // Convertire il messaggio in byte
        var messageBytes = Encoding.UTF8.GetBytes(message);

        // Firma digitale del messaggio
        var signedBytes = rsa.SignData(messageBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

        // Convertire il risultato in Base64
        return Convert.ToBase64String(signedBytes);
    }

    /// <summary>
    /// Esempio di crittografia con certificato ECC.
    /// </summary>
    /// <param name="certificate">Il certificato caricato.</param>
    /// <param name="message">Il messaggio da criptare.</param>
    /// <returns>Il messaggio crittografato come Base64.</returns>
    private string EncryptWithECDsa(X509Certificate2 certificate, string message, bool privateKey)
    {
        if (certificate.HasPrivateKey && !privateKey)
            throw new InvalidOperationException("The certificate provided has a private key. Please use a certificate with a public key only.");
        if (!certificate.HasPrivateKey && privateKey)
            throw new InvalidOperationException("The certificate provided hasn't a private key. Please use a certificate with a private key only.");

        // Ottenere la chiave pubblica ECDsa
        using var ecdsa = privateKey ? certificate.GetECDsaPrivateKey() : certificate.GetECDsaPublicKey();
        if (ecdsa == null)
            throw new InvalidOperationException("No valid ECDsa public/private key found in the certificate.");

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
            case CertificateTypes.RSA_WithPublicKey when certificate.GetRSAPublicKey() == null:
                throw new InvalidOperationException("The certificate provided is not compatible with RSA public key encryption.");

            case CertificateTypes.RSA_WithPrivateKey when certificate.GetRSAPrivateKey() == null:
                throw new InvalidOperationException("The certificate provided is not compatible with RSA private key encryption.");

            case CertificateTypes.DSA_WithPublicKey when certificate.GetDSAPublicKey() == null:
                throw new InvalidOperationException("The certificate provided is not compatible with DSA public key encryption.");

            case CertificateTypes.DSA_WithPrivateKey when certificate.GetDSAPrivateKey() == null:
                throw new InvalidOperationException("The certificate provided is not compatible with DSA private key encryption.");

            case CertificateTypes.ECDsa_WithPublicKey when certificate.GetECDsaPublicKey() == null:
                throw new InvalidOperationException("The certificate provided is not compatible with ECDsa public key encryption.");

            case CertificateTypes.ECDsa_WithPrivateKey when certificate.GetECDsaPrivateKey() == null:
                throw new InvalidOperationException("The certificate provided is not compatible with ECDsa private key encryption.");

            case CertificateTypes.RSA_WithPublicKey or CertificateTypes.RSA_WithPrivateKey or
                CertificateTypes.DSA_WithPublicKey or CertificateTypes.DSA_WithPrivateKey or
                CertificateTypes.ECDsa_WithPublicKey or CertificateTypes.ECDsa_WithPrivateKey:
                break; // Nessun errore, il tipo di certificato è valido.

            default:
                throw new NotSupportedException($"Tipo di certificato non supportato: {certificateType.ToString()}");
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
    private readonly ISerilogConfiguration _configuration;

    public RawTcpSink(ISerilogConfiguration configuration)
    {
        _configuration = configuration;
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
            // Converti il messaggio in byte e invia
            var data = Encoding.UTF8.GetBytes(message + "\n"); // Aggiungi newline per il server

            // Creazione di un TcpClient dedicato per ogni connessione
            _ = Task.Run(async () =>
            {
                using var client = new TcpClient();
                try
                {
                    await client.ConnectAsync(_configuration.ServerAddress, int.Parse(_configuration.ServerPort));
                    using var stream = client.GetStream();
                    await stream.WriteAsync(data, 0, data.Length);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Errore durante l'invio del messaggio: {ex.Message}");
                }
            }).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            switch (_configuration.LogTypes) 
            {
                case ISegugioProvider.LogTypes.Console:
                    Console.WriteLine(ex);
                    break;
                case ISegugioProvider.LogTypes.Exception:
                    throw new SegugioException("Generic error in SerilogProvider", ex);
            };
        }
    }
}

public enum CertificateTypes
{
    None,
    RSA_WithPrivateKey,
    RSA_WithPublicKey,
    DSA_WithPrivateKey,
    DSA_WithPublicKey,
    ECDsa_WithPrivateKey,
    ECDsa_WithPublicKey
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