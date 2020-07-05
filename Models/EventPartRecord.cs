using System;
using Orchard.ContentManagement.Records;

namespace Codesanook.EventManagement.Models {
    public class EventPartRecord : ContentPartRecord {
        public virtual string Location { get; set; }
        public virtual DateTime? StartDateTimeUtc { get; set; }
        public virtual DateTime? FinishDateTimeUtc { get; set; }
        public virtual int MaxAttendees { get; set; }
        public virtual decimal TicketPrice { get; set; }

        //TODO for query only?
        //public virtual int ContentItemId { get; set; }
    }
}
