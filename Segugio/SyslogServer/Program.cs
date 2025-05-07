using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SyslogServer
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            TcpListener tcpListener = new TcpListener(IPAddress.Any, 514);
            tcpListener.Start();
            Console.WriteLine("Fake Syslog server listining on port 514...");

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
                Console.WriteLine($"Received message from {tcpClient.Client.RemoteEndPoint}: {message}");
            }
            // UdpClient udpClient = new UdpClient(514);
            // Console.WriteLine("Fake Syslog server listening on port 514...");
            //
            // while (true)
            // {
            //     UdpReceiveResult result = await udpClient.ReceiveAsync();
            //     string message = Encoding.ASCII.GetString(result.Buffer);
            //     Console.WriteLine($"Received message from {result.RemoteEndPoint}: {message}");
            // }
        }
    }
}