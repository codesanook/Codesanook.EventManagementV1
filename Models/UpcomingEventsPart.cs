using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;

namespace Codesanook.EventManagement.Models {
    public class UpcomingEventsPart : ContentPart {

        [Required]
        public int Count {
            // Use infoset only
            get => this.Retrieve(x => x.Count);
            set => this.Store(x => x.Count, value);
        }
    }
}
