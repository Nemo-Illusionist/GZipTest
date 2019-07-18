using System;
using System.IO;
using System.IO.Compression;
using GZipLib;
using GZipLib.Compressor;
using GZipLib.Reader;
using GZipLib.Settings;
using GZipLib.Writer;

namespace GZipTest
{
    class Program
    {
        static void Main(string[] args)
        {
//            args = new string[3];
//            args[0] = "compress";
//            args[1] = "C:/Users/illus/RiderProjects/original1.txt";
//            args[2] = "C:/Users/illus/RiderProjects/original11.gz";
//            
//            args[0] = "decompress";
//            args[1] = "C:/Users/illus/RiderProjects/original11.gz";
//            args[2] = "C:/Users/illus/RiderProjects/original111.txt";
            

            try
            {
                var settings = CompressorSettingsManager.GetOrDefault("appsettings.xml");

                var mode = GetMode(args, settings);
                var input = GetInput(args, settings);
                var output = GetOutput(args, settings);

                var compressor = new GZipCompressor(settings.BufferSize);
                var readerQueueFactory = new FileReaderQueueFactory(input, settings);
                var writerQueue = new WriterQueue(new FileWriter(output));

                using (var manager = new CompressorManager(writerQueue, readerQueueFactory, compressor, settings))
                {
                    manager.Run(mode);
                    manager.Join();
                }

                Environment.Exit(0);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Environment.Exit(1);
            }
        }

        private static string GetOutput(string[] args, CompressorSettings settings)
        {
            var output = args[settings.OutputIndex];
            if (output.IndexOfAny(Path.GetInvalidPathChars()) != -1) throw new ArgumentException("Path is Invalid");
            if (File.Exists(output)) throw new ArgumentException("File already exists");
            return output;
        }

        private static string GetInput(string[] args, CompressorSettings settings)
        {
            var input = args[settings.InputIndex];
            if (input.IndexOfAny(Path.GetInvalidPathChars()) != -1) throw new ArgumentException("Path is Invalid");
            if (!File.Exists(input)) throw new ArgumentException("Fail is not exists");
            return input;
        }

        private static CompressionMode GetMode(string[] args, CompressorSettings settings)
        {
            CompressionMode mode;
            var modeText = args[settings.ModIndex].ToLower();
            switch (modeText)
            {
                case "compress":
                    mode = CompressionMode.Compress;
                    break;
                case "decompress":
                    mode = CompressionMode.Decompress;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(args), args[0],
                        "Please choose one of the two: compress or decompress");
            }

            return mode;
        }
    }
}