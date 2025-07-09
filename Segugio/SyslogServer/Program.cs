using System;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SyslogServer
{
    internal enum ServerTypes
    {
        TCP,
        TCPWithSSL,
        UDP
    }

    internal class Program
    {
        public static async Task Main(string[] args)
        {
            var serverType = ServerTypes.TCP;
            
            if (serverType == ServerTypes.TCPWithSSL || serverType == ServerTypes.TCP)
                await StartTcpListenerAndProcessMessages(serverType);
            else if (serverType == ServerTypes.UDP)
                await StartUdpListenerAndProcessMessages(serverType);
        }

        private static async Task StartUdpListenerAndProcessMessages(ServerTypes serverType)
        {
            UdpClient udpClient = new UdpClient(514);
            Console.WriteLine("Fake Syslog server listening on port 514...");
            
            while (true)
            {
                UdpReceiveResult result = await udpClient.ReceiveAsync();
                string message = Encoding.ASCII.GetString(result.Buffer);
                Console.WriteLine($"Received message from {result.RemoteEndPoint}: {message}");
            }
        }

        private static async Task StartTcpListenerAndProcessMessages(ServerTypes serverType)
        {
            TcpListener tcpListener = new TcpListener(IPAddress.Any, 514);
            tcpListener.Start();
            Console.WriteLine("Fake Syslog server listining on port 514...");
            var certificatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Certificati\\RSApublic.pfx");

            while (true)
            {
                using TcpClient tcpClient = tcpListener.AcceptTcpClient();
                using NetworkStream stream = tcpClient.GetStream();
                
                byte[] buffer = new byte[1024];
                // int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                // string message = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                
                int bytesRead;
                StringBuilder messageBuilder = new StringBuilder();
                while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    string messagePart = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    messageBuilder.Append(messagePart);
                
                    // Check if the message is complete (you might need a specific delimiter or condition)
                    if (messagePart.EndsWith("\n")) // Assuming messages end with a newline character
                    {
                        break;
                    }
                }
                string message = messageBuilder.ToString();
                if (serverType == ServerTypes.TCP)
                    Console.WriteLine($"Received message from {tcpClient.Client.RemoteEndPoint}: {message}");
                else if (serverType == ServerTypes.TCPWithSSL)
                {
                    string decryptedMessage = DecryptMessageWithCertificate(message, certificatePath);
                    Console.WriteLine($"Received message from {tcpClient.Client.RemoteEndPoint}: {decryptedMessage}");
                }
            }
        }

        /// <summary>
        /// Decripta un messaggio ricevuto utilizzando un certificato con una chiave privata.
        /// </summary>
        /// <param name="encryptedMessage">Il messaggio criptato in Base64.</param>
        /// <param name="certificatePath">Il percorso del certificato (che contiene la chiave privata).</param>
        /// <returns>Il messaggio decriptato in chiaro.</returns>
        private static string DecryptMessageWithCertificate(string encryptedMessage, string certificatePath)
        {
            if (string.IsNullOrWhiteSpace(certificatePath) || !File.Exists(certificatePath))
            {
                throw new FileNotFoundException("Certificate file not found at specified path.", certificatePath);
            }
    
            // Caricare il certificato
            var certificate = new X509Certificate2(certificatePath, "testsegugio", X509KeyStorageFlags.Exportable);
            using var rsa = certificate.GetRSAPrivateKey();
    
            if (rsa == null)
            {
                throw new InvalidOperationException("No valid RSA private key found in the certificate.");
            }

            // Convertire il messaggio Base64 in byte
            var encryptedBytes = Convert.FromBase64String(encryptedMessage);

            // Decriptare i dati
            var decryptedBytes = rsa.Decrypt(encryptedBytes, RSAEncryptionPadding.OaepSHA256);

            // Convertire in stringa
            return Encoding.UTF8.GetString(decryptedBytes);
        }
    }
}