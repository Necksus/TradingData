namespace IPTMGrabber.Edgar
{
    internal class Filling
    {
        public DateTime? AcceptanceDatetime { get; }
        public string FileName { get; }
        public string Type { get; }
        public DateTime? FiledAsOfDate { get; }
        public string ItemInformation { get; }

        public Filling(DateTime? acceptanceDatetime, string fileName, string type, DateTime? filedAsOfDate, string itemInformation)
        {
            AcceptanceDatetime = acceptanceDatetime;
            FileName = fileName;
            Type = type;
            FiledAsOfDate = filedAsOfDate;
            ItemInformation = itemInformation;
        }

        public override string ToString()
            => $"{AcceptanceDatetime} - {Type} - {ItemInformation}";
    }
}