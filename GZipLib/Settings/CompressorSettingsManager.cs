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
                    PageSize = Constants.PageSize, 
                    BufferSize = Constants.BufferSize,
                    ThreadPoolSize = Constants.ThreadPoolSize, 
                    ModIndex = Constants.ModIndex, 
                    InputIndex = Constants.InputIndex,
                    OutputIndex = Constants.OutputIndex,
                };
            }

            return settings;
        }
    }
}
