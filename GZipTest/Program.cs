using System;
using System.IO;
using System.IO.Compression;
using GZipLib;
using GZipLib.Compressor;
using GZipLib.Queue;
using GZipLib.Reader;
using GZipLib.Settings;
using GZipLib.Writer;

namespace GZipTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var exitCode = 1;
            try
            {
                var settings = CompressorSettingsManager.GetOrDefault("appsettings.xml");

                CompressionMode mode;
                string input;
                string output;
                try
                {
                    mode = GetMode(args, settings);
                    input = GetInput(args, settings);
                    output = GetOutput(args, settings);
                }
                catch (IndexOutOfRangeException e)
                {
                    throw new Exception("Please check the parameters: mode input output", e);
                }

                var compressor = new GZipCompressor(settings.BufferSize);
                var readerJobFactory = new FileReaderJobFactory(input, settings);
                var writerJobFactory = new FileWriterJobFactory(output);

                var readerQueue = new Queue();
                var writerQueue = new Queue();

                using (var manager = new CompressorManager(readerQueue, writerQueue,
                    writerJobFactory, readerJobFactory, compressor, settings))
                {
                    manager.Run(mode);
                    manager.Join();
                }

                exitCode = 0;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            Environment.Exit(exitCode);
        }

        private static string GetOutput(string[] args, CompressorSettings settings)
        {
            var output = args[settings.OutputIndex];
            if (output.IndexOfAny(Path.GetInvalidPathChars()) != -1) throw new ArgumentException("Path is Invalid");
            if (File.Exists(output)) throw new ArgumentException("Output file already exists");
            return output;
        }

        private static string GetInput(string[] args, CompressorSettings settings)
        {
            var input = args[settings.InputIndex];
            if (input.IndexOfAny(Path.GetInvalidPathChars()) != -1) throw new ArgumentException("Path is Invalid");
            if (!File.Exists(input)) throw new ArgumentException("Input fail is not exists");
            return input;
        }

        private static CompressionMode GetMode(string[] args, CompressorSettings settings)
        {
            var modeText = args[settings.ModIndex].ToLower();
            CompressionMode mode;
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