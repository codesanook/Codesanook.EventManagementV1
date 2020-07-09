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
        public decimal GetTotalPrice() {
            return Math.Round(EventAttendees.Count * Event.TicketPrice,2);
        }

        public string GetTextFromNow() {

            var toDay = DateTime.Now;
            if (BookingDateTimeUtc.HasValue) {
                int daysDiff = (toDay.Date - BookingDateTimeUtc.Value.Date).Days;
                if (daysDiff > 1) {
                    return daysDiff + " days ago";
                }

                return "today";
            }

            return "";
        }
    }
}