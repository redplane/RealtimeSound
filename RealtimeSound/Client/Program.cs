using System;
using System.Dynamic;
using System.IO;
using System.Media;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using WMPLib;

namespace Client
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                // Name of sound file.
                var fileName = "Assets\\Nguoi ta noi.wav";

                // Name of path.
                var applicationPath = Assembly.GetExecutingAssembly().Location;

                // Application path not found.
                if (string.IsNullOrWhiteSpace(applicationPath))
                {
                    Console.WriteLine("Application path is not valid.");
                    Console.ReadLine();
                    return;
                }

                // Get directory name.
                applicationPath = Path.GetDirectoryName(applicationPath);

                if (string.IsNullOrWhiteSpace(applicationPath))
                {
                    Console.WriteLine("Application path is not valid.");
                    Console.ReadLine();
                    return;
                }

                // Get full path.
                var fullPath = Path.Combine(applicationPath, fileName);

                // File doesn't exist.
                if (!File.Exists(fullPath))
                {
                    Console.WriteLine($"{fullPath} doesn't exist.");
                    Console.ReadLine();
                    return;
                }

                var ipAddress = IPAddress.Any;
                var iPort = 9003;
                Console.WriteLine($"Connecting to port {iPort}");
                
                var tcpClient = new TcpClient();
                tcpClient.Connect("127.0.0.1", iPort);
                Console.WriteLine("Connected");

                var networkStream = tcpClient.GetStream();
                var broadcastingThread = new Thread(() =>
                {
                    var buffers = new byte[4 * 1000000];
                    using (var fileStream = File.OpenRead(fullPath))
                    {
                        var iReadBytes = 0;
                        while ((iReadBytes = fileStream.Read(buffers, 0, buffers.Length)) > 0)
                        {
                            networkStream.Write(buffers, 0, iReadBytes);
                            //networkStream.Flush();
                            Console.WriteLine($"Sent {iReadBytes} bytes");
                            Thread.Sleep(TimeSpan.FromSeconds(10));
                        }

                        Console.WriteLine("Reading");
                        Console.ReadLine();
                    }
                });

                broadcastingThread.IsBackground = true;
                broadcastingThread.Start();

                Console.ReadLine();

            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                Console.ReadLine();
                return;
            }
        }
    }
}