using System.IO;
using System.Xml.Serialization;

namespace TimeStamper
{
    public class Configuration
    {
        public bool ShouldOutputFooter;
        public string StandardOutputSequence;
        public string StandardErrorSequence;
        public string InformationalSequence;
        public string TimeStampFormat;
        private readonly string _configFile;
        private readonly XmlSerializer _serializer = new XmlSerializer(typeof(Configuration));

        public Configuration(string configDirectory)
        {
            _configFile = Path.Combine(configDirectory, "timestamper.xml");
            Directory.CreateDirectory(configDirectory);
            LoadConfiguration();
        }

        public Configuration() { }

        private void LoadConfiguration()
        {
            if (!File.Exists(_configFile))
            {
                SetDefaults();
                SaveConfiguration();
                return;
            }

            TextReader reader = new StreamReader(_configFile);
            var config = _serializer.Deserialize(reader) as Configuration;
            reader.Close();
            ShouldOutputFooter = config.ShouldOutputFooter;
            StandardOutputSequence = config.StandardOutputSequence;
            StandardErrorSequence = config.StandardErrorSequence;
            InformationalSequence = config.InformationalSequence;
            TimeStampFormat = config.TimeStampFormat;
        }

        private void SetDefaults()
        {
            ShouldOutputFooter = true;
            StandardOutputSequence = "92";
            StandardErrorSequence = "91";
            InformationalSequence = "96";
            TimeStampFormat = "[yyyy-MM-dd HH:mm:ss] ";
        }
        private void SaveConfiguration()
        {
            TextWriter writer = new StreamWriter(_configFile);
            _serializer.Serialize(writer, this);
            writer.Close();
        }
    }
}
