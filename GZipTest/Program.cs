using GZipLib;

namespace GZipTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var input = "./../../original1.txt";
            var output = "./../../original11.gz";

            var compressorManager = new CompressorManager(input, output);
            compressorManager.Compress();
            compressorManager.Join();
            
        }
    }
}