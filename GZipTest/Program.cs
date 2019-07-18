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
//            var input = "./../../original1.txt";
//            var output = "./../../original11.gz";      
//            var mode = CompressionMode.Compress;

            var input = "./../../original11.gz";
            var output = "./../../original111.txt";
            var mode = CompressionMode.Decompress;

            var compressorSettings = CompressorSettings.Default();
            var compressor = new GZipCompressor(compressorSettings.BufferSize);
            var readerQueueFactory = new FileReaderQueueFactory(input, compressorSettings);
            var writerQueue = new WriterQueue(new FileWriter(output));

            using (var manager = new CompressorManager(writerQueue, readerQueueFactory, compressor, compressorSettings))
            {
                manager.Run(mode);
                manager.Join();
            }
        }
    }
}
