using Orchard.Core.Common.ViewModels;

namespace Codesanook.EventManagement.ViewModels {
    public class EventViewModel {
        public DateTimeEditor BeginDateTimeEditor { get; set; }
        public DateTimeEditor EndDateTimeEditor { get; set; }
        public string Location { get; set; }
        public int MaxAttendees { get; set; }
    }
}
