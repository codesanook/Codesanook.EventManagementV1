using System;
using Orchard.ContentManagement.Records;

namespace Codesanook.EventManagement.Models {
    public class EventPartRecord : ContentPartRecord {
        public virtual string Location { get; set; }
        public virtual DateTime? BeginDateTimeUtc { get; set; }
        public virtual DateTime? EndDateTimeUtc { get; set; }
        public virtual int MaxAttendees { get; set; }

        //TODO for query only?
        //public virtual int ContentItemId { get; set; }
    }
}
