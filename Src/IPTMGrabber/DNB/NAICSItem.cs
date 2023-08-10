namespace IPTMGrabber.DNB
{
    internal class NAICSItem
    {
        public string Code { get; set; }
        public string Name { get; set; }

        public override string ToString() => $"{Code} - {Name}";
    }
}
