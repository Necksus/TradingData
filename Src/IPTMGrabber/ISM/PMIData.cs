namespace IPTMGrabber.ISM
{
    internal class PMIData
    {
        public string Name { get; }
        public IEnumerable<(string Key, string Value)> Values { get; }

        public PMIData(string name, IEnumerable<(string Key, string Value)> values)
        {
            Name = name;
            Values = values;
        }
    }
}
