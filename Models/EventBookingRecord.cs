using System;
using System.Collections.Generic;
using Orchard.Users.Models;

namespace Codesanook.EventManagement.Models {
    /// <summary>
    ///  Non content part model
    /// </summary>
    public class EventBookingRecord {

        private IList<EventAttendeeRecord> eventAttendees;
        public EventBookingRecord() =>
            eventAttendees = new List<EventAttendeeRecord>();

        public virtual int Id { get; set; }
        public virtual EventPartRecord Event { get; set; }
        public virtual UserPartRecord User { get; set; }
        public virtual DateTime? BookingDateTimeUtc { get; set; }

        // Todo use enum but enum string 
        public virtual EventBookingStatus Status { get; set; }
        public virtual DateTime? PaidDateTimeUtc { get; set; }
        public virtual string PaymentConfirmationAttachementFileUrl { get; set; }

        // Uni-directional mapping and get only
        public virtual IList<EventAttendeeRecord> EventAttendees => eventAttendees;
        public void AddEventAttendee(EventAttendeeRecord eventAttendee) => eventAttendees.Add(eventAttendee);
    }
}