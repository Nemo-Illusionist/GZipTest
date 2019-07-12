using System.IO;
using GZipLib.Compressor;
using GZipLib.Reader;
using GZipLib.Writer;

namespace GZipTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var input = "./../../original1.txt";
            var output = "./../../original1.gz";

            var info = new FileInfo(input);
            var pageSize = 4194304;
            var bufferSize = 81920;

            var fileLength = info.Length;
            var leftBytes = fileLength;
            var index = 0;

            var fileWriter = new FileWriter(output);
            var fileReader = new FileReader(input);
            var gZipCompressor = new GZipCompressor(bufferSize);
            try
            {
                var writelQueue = WritelQueue.CreateAndStart(fileLength / pageSize, fileWriter);

                while (leftBytes > 0)
                {
                    var length = leftBytes < pageSize ? (int) leftBytes : pageSize;
                    var position = fileLength - leftBytes;

                    var bytes = fileReader.Read((int) position, length);
                    bytes = gZipCompressor.Compress(bytes);
                    writelQueue.Add(index, bytes);

                    leftBytes -= length;
                    index++;
                }

                writelQueue.Join();
            }
            finally
            {
                fileWriter.Dispose();
            }
        }
    }
}