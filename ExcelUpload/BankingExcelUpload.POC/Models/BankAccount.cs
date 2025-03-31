using System;

namespace BankingExcelUpload.POC.Models
{
    public class BankAccount
    {
        public string AccountHolderName { get; set; }
        public string AccountNumber { get; set; }
        public string BankName { get; set; }
        public string IFSCCode { get; set; }
        public string Branch { get; set; }
        public DateTime OpeningDate { get; set; }
    }
}
