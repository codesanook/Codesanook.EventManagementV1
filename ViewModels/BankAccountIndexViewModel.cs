using System.Collections.Generic;
using Codesanook.EventManagement.Models;

namespace Codesanook.EventManagement.ViewModels {

    public class BankAccountIndexViewModel {
        public IReadOnlyCollection<BankAccountRecord> BankAccounts { get; set; }
        public dynamic Pager { get; set; }
    }
}
