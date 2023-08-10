namespace IPTMGrabber.InvestorWebsite
{
    internal class EventInfo
    {
        public DateTime Date { get; }
        public string? Description { get; }
        public string Link { get; }

        public EventInfo(DateTime date, string? description, string link)
        {
            Date = date;
            Description = description;
            Link = link;
        }

        public override string ToString()
            => $"{Date:dd/MM/yyyy} : {Description}";
    }
}
