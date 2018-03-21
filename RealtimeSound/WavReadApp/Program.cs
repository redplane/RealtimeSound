using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NAudio.Wave;

namespace WavReadApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var fileName = "Assets\\Nguoi ta noi.wav";
            var applicationPath = Assembly.GetExecutingAssembly().Location;

            if (string.IsNullOrWhiteSpace(applicationPath))
            {
                Console.WriteLine("Application path is invalid.");
                Console.ReadLine();
                return;
            }

            // Get full path.
            var fullPath = Path.GetDirectoryName(applicationPath);
            if (string.IsNullOrWhiteSpace(fullPath))
            {
                Console.WriteLine("Full path is not valid.");
                Console.ReadLine();
                return;
            }

            // Get file path.
            var filePath = Path.Combine(fullPath, fileName);
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                Console.WriteLine("File doesn't exist.");
                Console.ReadLine();
                return;
            }

            byte[] bytes = new byte[1024];

            
            var waveOut = new WaveOut();
            

            //var soundPlayer = new  SoundPlayer();
            //soundPlayer.Stream = new MemoryStream();
            //soundPlayer.Load();
            
            var bufferedWaveProvider = new BufferedWaveProvider(new WaveFormat());
            bufferedWaveProvider.DiscardOnBufferOverflow = true;
            bufferedWaveProvider.BufferDuration = TimeSpan.FromSeconds(10);
            bufferedWaveProvider.ReadFully = false;

            var buffer = new byte[2048];

            //IWaveProvider provider = new RawSourceWaveStream(ms, new WaveFormat(48000, 16, 1));
            waveOut.Init(bufferedWaveProvider);
            waveOut.Play();

            using (var fileStream = File.OpenRead(filePath))
            {
                while (true)
                {
                    try
                    {
                        
                        var iReadBytes = fileStream.Read(buffer, 0, buffer.Length);
                        if (iReadBytes < 1)
                        {
                            Console.WriteLine("Offset end.");
                            break;
                        }

                        //ms.Write(buffer, 0, iReadBytes);
                        //soundPlayer.Stream.Write(buffer, (int) soundPlayer.Stream.Length, iReadBytes);
                        bufferedWaveProvider.AddSamples(buffer, 0, iReadBytes);
                        //soundPlayer.Stream.Seek(0, SeekOrigin.Begin);
                        //var memoryStream = new MemoryStream(buffer, 0, iReadBytes);
                        //soundPlayer.Stream = memoryStream;
                        //soundPlayer.Load();
                        //soundPlayer.Play();


                        //if (!bIsStarted)
                        //{
                        //    soundPlayer.Play();
                        //    bIsStarted = true;
                        //}

                        //Thread.Sleep(TimeSpan.FromSeconds(5));
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine(exception.Message);
                        break;
                    }
                }
            }

            Console.ReadLine();
        }
    }
}
