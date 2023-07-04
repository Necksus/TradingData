namespace IPTMGrabber.Utils
{
    [AttributeUsage(AttributeTargets.Property)]
    public class TableColumnAttribute : Attribute
    {
        public int ColumnIndex { get; }

        public TableColumnAttribute(int columnIndex)
        {
            ColumnIndex = columnIndex;
        }
    }
}
