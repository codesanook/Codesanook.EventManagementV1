using System;
using Orchard.DisplayManagement.Shapes;

namespace Codesanook.EventManagement.ViewModels {
    public class EventDisplayViewModel : Shape {
        public int EventId { get; set; }
        public DateTime BeginDateTimeUtc { get; set; }
        public DateTime EndDateTimeUtc { get; set; }
        public string Location { get; set; }

        // In a context of a ticket
        public decimal TicketPrice { get; set; }
        public int AvailableTicketCount { get; set; }
    }
}