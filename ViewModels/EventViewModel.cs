using Orchard.Core.Common.ViewModels;

namespace Codesanook.EventManagement.ViewModels {
    public class EventViewModel {
        public DateTimeEditor BeginDateEditor { get; set; }
        public DateTimeEditor EndDateEditor { get; set; }

        public DateTimeEditor BeginTimeEditor { get; set; }
        public DateTimeEditor EndTimeEditor { get; set; }

        public string Location { get; set; }
        public int MaxAttendees { get; set; }
        public decimal TicketPrice { get; set; }
    }
}
