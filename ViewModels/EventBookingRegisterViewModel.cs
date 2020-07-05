using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Codesanook.EventManagement.Models;

namespace Codesanook.EventManagement.ViewModels {
    public class EventBookingRegisterViewModel {
        public EventPart Event { get; set; }
        public IList<EventAttendeeRecord> EventAttendees { get; set; }
    }
}