using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Media;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using NAudio.Wave;

namespace Server
{
    internal class Program
    {
        private static Thread _connectionListeningThread;

        private static List<Thread> _connectionListenerThreads;

        private static void Main(string[] args)
        {
            _connectionListenerThreads = new List<Thread>();

            // Get server port.
            var iServerPort = GetServerPort();

            // Initialize tcp listener.
            var tcpListener = new TcpListener(IPAddress.Any, iServerPort);
            tcpListener.Start();

            _connectionListeningThread = new Thread(() => ListenIncomingConnection(tcpListener));
            _connectionListeningThread.IsBackground = true;
            _connectionListeningThread.Start();

            Console.ReadLine();
        }

        /// <summary>
        /// Get port.
        /// </summary>
        /// <returns></returns>
        private static int GetServerPort()
        {
            Console.Write("Server port: ");
            var szServerPort = Console.ReadLine();

            if (int.TryParse(szServerPort, out var iPort))
                return iPort;

            return iPort;
        }

        /// <summary>
        /// Listen to incoming connection.
        /// </summary>
        /// <param name="tcpListener"></param>
        private static void ListenIncomingConnection(TcpListener tcpListener)
        {
            Console.WriteLine("Waiting for incoming connections...");
            var tcpClient = tcpListener.AcceptTcpClient();
            Console.WriteLine("Caught a connection.");
            var connectionListenerThread = new Thread(new ParameterizedThreadStart(o =>
            {
                var buffer = new byte[2048];
                var networkStream = tcpClient.GetStream();
                var soundPlayer = new SoundPlayer();
                
                while (true)
                {
                    try
                    {
                        while (true)
                        {
                            //var iIndex = networkStream.Read(buffer, 0, buffer.Length);
                            //if (iIndex != 0)
                            if (true)
                            {
                                Console.WriteLine("Received data from client");
                                var memoryStream = new MemoryStream();
                                networkStream.CopyTo(memoryStream);
                                memoryStream.Seek(0, SeekOrigin.Begin);
                                soundPlayer.Stream = memoryStream;
                                soundPlayer.Load();
                                soundPlayer.Play();




                                Thread.Sleep(TimeSpan.FromSeconds(5));
                                break;
#if DEBUG
                                Console.WriteLine(buffer);
#endif
                                continue;
                            }
                            break;
                        }

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                }
            }));

            connectionListenerThread.IsBackground = true;
            _connectionListenerThreads.Add(connectionListenerThread);
            connectionListenerThread.Start();
        }
    }
}