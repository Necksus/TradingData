using HtmlAgilityPack;

namespace IPTMGrabber.InvestorWebsite
{
    internal class TargetNode<T>
    {
        public T Value { get; }
        public HtmlNode Node { get; }
        public int Level { get; }

        public TargetNode(T value, HtmlNode node, int level)
        {
            Value = value;
            Node = node;
            Level = level;
        }

        public override string ToString()
            => $"{Level} : {Value}";
    }
}
