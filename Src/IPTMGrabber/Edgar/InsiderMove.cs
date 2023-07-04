using CsvHelper.Configuration.Attributes;
using IPTMGrabber.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPTMGrabber.Edgar
{
    public enum MoveType
    {
        Unknown,
        A,
        D
    }

    public enum TransactionType
    {
        Award,
        Conversion,
        Return,
        ExpireShort,
        InKind,
        Gift,
        ExpireLong,
        Discretionary,
        Other,
        Small,
        Exempt,
        OutOfTheMoney,
        Purchase,
        Sale,
        Tender,
        Will,
        InTheMoney,
        Trust
    }

    internal class InsiderMove
    {
        [TableColumn(0)]
        public MoveType MoveType { get; set; }

        [TableColumn(1)]
        public DateTime Date { get; set; }

//        [TableColumn(2)]
//        public DateTime DeemedDate { get; set; }

        [TableColumn(3)] 
        public string ReportingOwner { get; set; }

        [TableColumn(4)]
        public string Form { get; set; }

        [TableColumn(5)]
        [Name("TransactionType")]
        public string TransactionTypeFull { get; set; }
        
        [TableColumn(6)]
        public string DirectOrIndirect { get; set; }

        [TableColumn(7)]
        public double NumberOfSecuritiesTransacted { get; set; }

        [TableColumn(8)]
        public double NumberOfSecuritiesOwned { get; set; }

        [TableColumn(9)]
        public int LineNumber { get; set; }

        [TableColumn(10)]
        public string OwnerCIK { get; set; }

        [TableColumn(11)]
        public string SecurityName { get; set; }

        [Ignore]
        public TransactionType TransactionType 
            => Enum.Parse<TransactionType>(TransactionTypeFull.Substring(2));
    }
}
