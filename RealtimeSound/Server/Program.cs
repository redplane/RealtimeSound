using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using NAudio.Wave;

namespace Server
{
    internal class Program
    {
        #region Properties

        /// <summary>
        /// Thread to listen to tcp connection.
        /// </summary>
        private static Thread _tcpConnectionListeningThread;

        /// <summary>
        /// Thread which is for checking buffer.
        /// </summary>
        private static Thread _waveBufferCheckThread;

        /// <summary>
        /// Tcp client which is for receiving data.
        /// </summary>
        private static TcpClient _tcpClient;

        /// <summary>
        /// Size of buffer (in MB)
        /// </summary>
        private const int BufferSize = 10;

        // Conversion from 1 Mb to B.
        private const int MbToB = 1000000;

        // Buffer size (in byte)
        private static int iBufferBytes = BufferSize * MbToB;

        /// <summary>
        /// Instance for playing audio bytes.
        /// </summary>
        private static WaveOut _waveOut;

        /// <summary>
        /// Instance of buffer which is for buffering wave stream.
        /// </summary>
        private static BufferedWaveProvider _bufferedWaveProvider;

        /// <summary>
        /// In-memory stream to store data.
        /// </summary>
        private static List<byte> _bytes;

        #endregion
        
        //private static MemoryStream _memoryStream
        private static void Main(string[] args)
        {
            // Get server port.
            var iServerPort = GetServerPort();

            // Initialize memory stream.
            _bytes = new List<byte>();

            // Initialize tcp listener.
            var tcpListener = new TcpListener(IPAddress.Any, iServerPort);
            tcpListener.Start();

            // Accept a tcp client.
            Console.WriteLine("Waiting for incoming connection");
            _tcpClient = tcpListener.AcceptTcpClient();
            Console.WriteLine("Client found !");

            // Buffered wave provider.
            _bufferedWaveProvider = new BufferedWaveProvider(new WaveFormat());
            _bufferedWaveProvider.DiscardOnBufferOverflow = true;
            _bufferedWaveProvider.BufferLength = iBufferBytes;
            
            _waveOut = new WaveOut();
            _waveOut.Init(_bufferedWaveProvider);
            _waveOut.Play();

            // Initialize a thread to listen to incoming data from another source.
            _tcpConnectionListeningThread = new Thread(() =>
            {
                // Get data stream of tcp connection.
                var networkStream = _tcpClient.GetStream();

                // Initalize buffer.
                var buffer = new byte[iBufferBytes];

                // Number of bytes which have been read.

                while (true)
                {
                    try
                    {
                        int iReadBytes;
                        while ((iReadBytes = networkStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            //var memoryStream = new MemoryStream();
                            //memoryStream.Write(buffer, 0, iReadBytes);
                            //_memoryStreams.Enqueue(memoryStream);
                            //_memoryStream.CopyTo();
                            //_memoryStream.Write(buffer, 0, iReadBytes);
                            var realBytes = buffer.Take(iReadBytes).ToList();
                            _bytes.AddRange(realBytes);
                            Console.WriteLine($"Read: {iReadBytes} bytes. Enqueue a stream. Count: {realBytes.Count}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                        break;
                    }
                }
            });

            // Initialize a thread to check audio buffer. Reload as audio stream consumes whole data.
            _waveBufferCheckThread = new Thread(() =>
            {
                while (true)
                {
                    // Get remaining bytes in buffer.
                    var iRemainingBytes = _bufferedWaveProvider.BufferedBytes;

                    // No byte is available in buffer. 
                    // Check memory stream to find available bytes array. If nothing found, terminate this thread.
                    if (iRemainingBytes >= iBufferBytes)
                        continue;
                    
                    // No byte available.
                    if (_bytes.Count < 1)
                    {
                        //Console.WriteLine("No data in memory stream");
                        continue;
                    }

                    // Get bytes array from memory stream.
                    var iDifferent = iBufferBytes - iRemainingBytes;
                    if (iDifferent > _bytes.Count)
                        iDifferent = _bytes.Count;

                    var bytes = _bytes.GetRange(0, iDifferent).ToArray();
                    _bytes.RemoveRange(0, iDifferent);

                    //var bytes = new byte[iBufferBytes];
                    _bufferedWaveProvider.AddSamples(bytes, 0, bytes.Length);
                    //Console.WriteLine($"Wrote {iReadBytes} to buffer. Count: {_memoryStream.Length}");
                    //Thread.Sleep(TimeSpan.FromSeconds(1));
                    //_waveOut.Play();
                }
            });

            _tcpConnectionListeningThread.IsBackground = true;
            _waveBufferCheckThread.IsBackground = true;

            _tcpConnectionListeningThread.Start();
            _waveBufferCheckThread.Start();

            Console.ReadLine();

            _tcpConnectionListeningThread.Abort();
            _waveBufferCheckThread.Abort();
            _bufferedWaveProvider.ClearBuffer();
            _waveOut.Stop();
            tcpListener.Stop();
            
        }

        /// <summary>
        /// Get port.
        /// </summary>
        /// <returns></returns>
        private static int GetServerPort()
        {
            //Console.Write("Server port: ");
            //var szServerPort = Console.ReadLine();

            //if (int.TryParse(szServerPort, out var iPort))
            //    return iPort;

            //return iPort;
            return 9003;
        }
    }
}