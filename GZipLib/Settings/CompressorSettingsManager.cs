using System.IO;
using System.Xml.Serialization;

namespace GZipLib.Settings
{
    public static class CompressorSettingsManager
    {
        public static CompressorSettings GetOrDefault(string filePath)
        {
            CompressorSettings settings;
            if (File.Exists(filePath))
            {
                var formatter = new XmlSerializer(typeof(CompressorSettings));
                using (var fs = new FileStream(filePath, FileMode.OpenOrCreate))
                {
                    settings = (CompressorSettings) formatter.Deserialize(fs);
                }
            }
            else
            {
                settings = new CompressorSettings
                {
                    PageSize = 4096 * 1024, BufferSize = 81920, ThreadPoolSize = 10,
                    ModIndex = 0, InputIndex = 1, OutputIndex = 2,
                };
            }

            return settings;
        }
    }
}