namespace IPTMGrabber.ISM
{
    internal class Sectors
    {
        public string Name { get; }
        public IReadOnlyDictionary<string, int> Industries { get; }

        public Sectors(string name, IReadOnlyDictionary<string, int> industries)
        {
            Name = name;
            Industries = industries;
        }
    }
}