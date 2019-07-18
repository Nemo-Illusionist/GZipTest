using System.IO;

namespace GZipLib.Extensions
{
    public static class StreamExtensions
    {
        public static void Copy(this Stream input, Stream output, int bufferSize)
        {
            var buffer = new byte[bufferSize];
            int bytesRead;
            while ((bytesRead = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, bytesRead);
            }
        }
    }
}