using System;
using System.Linq;
using Codesanook.EventManagement.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;

namespace Codesanook.EventManagement.Drivers {
    public class UpcomingEventsPartDriver : ContentPartDriver<UpcomingEventsPart> {
        private readonly IContentManager contentManager;

        public UpcomingEventsPartDriver(IContentManager contentManager) {
            this.contentManager = contentManager;
        }

        protected override DriverResult Display(UpcomingEventsPart part, string displayType, dynamic shapeHelper) {

            var events = contentManager.Query(VersionOptions.Published, "Event") // Event type
                .Join<EventPartRecord>()
                .Where(e => e.BeginDateTimeUtc > DateTime.UtcNow) // Future event
                .OrderByDescending(e => e.BeginDateTimeUtc)
                .Slice(0, part.Count)
                .Select(e => e.As<EventPart>());

            var eventListShape = shapeHelper.List();
            eventListShape.AddRange(events.Select(e => contentManager.BuildDisplay(e, "Summary")));

            return ContentShape(
                "Parts_UpcomingEvents",
                () => shapeHelper.Parts_UpcomingEvents(ContentItems: eventListShape)
            );
        }
    }
}
