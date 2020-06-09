using System;
using Orchard.DisplayManagement.Shapes;

namespace Codesanook.EventManagement.ViewModels {
    public class EventDisplayViewModel : Shape {
        public DateTime BeginDateTimeUtc { get; set; }
        public DateTime EndDateTimeUtc { get; set; }
        public string Location { get; set; }
        public int MaxAttendees { get; set; }
        public decimal TicketPrice { get; set; }
    }
}