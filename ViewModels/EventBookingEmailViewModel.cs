using Codesanook.EventManagement.Models;
using Codesanook.BasicUserProfile.Models;
using System.Collections.Generic;

namespace Codesanook.EventManagement.ViewModels {
    public class EventBookingEmailViewModel {
        public string SiteName { get; set; }
        public EventPart Event { get; set; }
        public UserProfilePart UserProfile { get; set; }
        public dynamic AdditionalInformation { get; set; }
        public string ContactEmail { get; set; }
        public IReadOnlyCollection<BankAccountRecord> BankAccounts { get; set; }
    }
}