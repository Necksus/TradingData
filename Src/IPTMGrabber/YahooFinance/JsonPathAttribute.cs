namespace IPTMGrabber.YahooFinance
{
    public class JsonPathAttribute : Attribute
    {
        public string Path { get; }

        public JsonPathAttribute(string path)
        {
            Path = path;
        }
    }
}
