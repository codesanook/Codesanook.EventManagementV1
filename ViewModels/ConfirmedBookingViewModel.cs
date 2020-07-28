using Codesanook.EventManagement.Models;
using Codesanook.BasicUserProfile.Models;

namespace Codesanook.EventManagement.ViewModels {
    public class ConfirmedBookingViewModel {
        public EventPart Event { get; set; }
        public UserProfilePart UserProfile { get; set; }
    }
}