using System.Collections.Generic;
using Codesanook.EventManagement.Models;

namespace Codesanook.EventManagement.ViewModels {
    public class EventBookingResultViewModel {
        public EventPart Event { get; set; }
        public IReadOnlyCollection<BankAccountRecord> BankAccounts { get; set; }
    }
}