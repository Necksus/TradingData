namespace IPTMGrabber.InvestorWebsite
{
    internal class EventInfo : IEquatable<EventInfo>
    {
        public DateTime Date { get; }
        public string? Description { get; }
        public string Link { get; }
        public bool? EarningRelated { get; }

        public EventInfo(DateTime date, string? description, string link, bool? earningRelated)
        {
            Date = date;
            Description = description;
            Link = link;
            EarningRelated = earningRelated;
        }

        public override string ToString()
            => $"{Date:dd/MM/yyyy} : {Description}";

        public bool Equals(EventInfo? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Date.Equals(other.Date) && 
                   Description == other.Description && 
                   Link == other.Link;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((EventInfo) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Date, Description, Link, EarningRelated);
        }
    }
}
