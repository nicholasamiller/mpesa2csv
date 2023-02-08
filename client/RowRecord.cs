using CsvHelper.Configuration.Attributes;

namespace MPesa2Csv.Web
{
    record RowRecord
    {

        [Name("Recipt Number")]
        public string? ReciptNo { get; set; }

        [Name("Completion Time (Kenya/Nairobi)")]
        public DateTime CompletionTime { get; set; }

        [Name("Details")]
        public string? Details { get; set; }

        [Name("Transaction Status")]
        public string? TransactionStatus { get; set; }

        [Name("Paid In")]
        public decimal PaidIn { get; set; }


        [Name("Withdrawn")]
        public decimal WithDrawn { get; set; }


        [Name("Balance")]
        public decimal Balance { get; set; }


    }
}