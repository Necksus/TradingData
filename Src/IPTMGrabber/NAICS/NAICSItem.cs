namespace IPTMGrabber.NAICS
{
    internal class NAICSItem
    {
        public string Code { get; set; }
        public string Name { get; set; }
        /*
        public NAICSItem(string code, string name)
        {
            Code = code;
            Name = name;
        }*/

        public override string ToString() => $"{Code} - {Name}";
    }
}
