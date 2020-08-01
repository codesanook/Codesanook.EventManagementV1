using System.ComponentModel;

namespace Codesanook.EventManagement.Models {
    public class BankAccountRecord {

        public virtual int Id { get; set; }

        [DisplayName("Bank name")]
        public virtual string BankName { get; set; }

        [DisplayName("Account name")]
        public virtual string AccountName { get; set; }

        [DisplayName("Account number")]
        public virtual string AccountNumber { get; set; }

        [DisplayName("Branch name")]
        public virtual string BranchName { get; set; }
    }
}