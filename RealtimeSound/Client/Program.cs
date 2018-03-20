using System;
using System.Dynamic;
using System.IO;
using System.Media;
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

                var iPointer = 0;
                var soundPlayer = new SoundPlayer();
                soundPlayer.Stream = new MemoryStream();

                using (var fileStream = File.OpenRead(fullPath))
                {
                    while (true)
                    {
                        var buffers = new byte[2048];
                        iPointer = fileStream.Read(buffers, iPointer, buffers.Length);
                        if (iPointer < 1)
                        {
                            Console.WriteLine("Read complete");
                            break;
                        }

                        soundPlayer.Stream.Seek(0, SeekOrigin.Begin);
                        soundPlayer.Stream.Write(buffers, 0, buffers.Length);
                        //soundPlayer.Load();
                        soundPlayer.Play();


                        Thread.Sleep(TimeSpan.FromSeconds(5));
                    }

                    Console.WriteLine("Reading");
                    Console.ReadLine();
                }
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