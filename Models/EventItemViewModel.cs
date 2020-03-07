using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;

namespace Codesanook.EventManagement.Models {
    public class EventItemViewModel {
        public ContentItem Event { get; set; }
        public ContentItem User { get; set; }
    }
}
