using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Codesanook.EventManagement.Models;
using Orchard.Users.Models;

namespace Codesanook.EventManagement.ViewModels {
    public class EventBookingViewModel {

        public int Id { get; set; }
        public EventPart Event { get; set; }
        public UserPart User { get; set; }
        public DateTime? BookingDateTimeUtc { get; set; }
        public EventBookingStatus Status { get; set; }
        public DateTime? PaidDateTimeUtc { get; set; }
        public string PaymentConfirmationAttachementFileUrl { get; set; }
        public IList<EventAttendeeRecord> EventAttendees { get; set; }
    }
}