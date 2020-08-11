using Codesanook.EventManagement.Models;
using Codesanook.BasicUserProfile.Models;
using System.Collections.Generic;

namespace Codesanook.EventManagement.ViewModels {
    public class ConfirmedBookingViewModel {
        public string SiteName { get; set; }
        public EventPart Event { get; set; }
        public UserProfilePart UserProfile { get; set; }
        public IReadOnlyCollection<BankAccountRecord> BankAccounts { get; set; }
    }
}