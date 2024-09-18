using System.IO;
using System.Xml.Serialization;

namespace TimeStamper
{
    public class Configuration
    {
        public bool ShouldOutputFooter;
        public bool ShouldOutputHeader;
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
            // Set the defaults regardless of if the file exists.
            SetDefaults();

            if (!File.Exists(_configFile))
            {
                SaveConfiguration();
                return;
            }

            TextReader reader = new StreamReader(_configFile);
            var config = _serializer.Deserialize(reader) as Configuration;
            reader.Close();
            // Override the defaults
            ShouldOutputFooter = config.ShouldOutputFooter;
            ShouldOutputFooter = config.ShouldOutputHeader;
            StandardOutputSequence = config.StandardOutputSequence;
            StandardErrorSequence = config.StandardErrorSequence;
            InformationalSequence = config.InformationalSequence;
            TimeStampFormat = config.TimeStampFormat;
        }

        private void SetDefaults()
        {
            ShouldOutputFooter = true;
            // Default the header output to false as it could contain sensitive information.
            ShouldOutputHeader = false;
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
