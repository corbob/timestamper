namespace TimeStamper
{
    public class Configuration
    {
        public bool ShouldOutputFooter = true;
        public string StandardOutputSequence = "92";
        public string StandardErrorSequence = "91";
        public string InformationalSequence = "96";
        public string TimeStampFormat = "[yyyy-MM-dd HH:mm:ss] ";
    }
}
