using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Codesanook.EventManagement.Models {
    public class BankAccountViewModel {

        [DisplayName("Bank name")]
        [Required]
        public string BankName { get; set; }

        [Required]
        [DisplayName("Account name")]
        public string AccountName { get; set; }

        [Required]
        [DisplayName("Account number")]
        public string AccountNumber { get; set; }

        [Required]
        [DisplayName("Branch name")]
        public string BranchName { get; set; }
    }
}