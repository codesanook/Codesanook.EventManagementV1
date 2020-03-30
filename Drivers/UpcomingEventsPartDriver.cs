using System;
using System.Linq;
using Codesanook.EventManagement.Models;
using Codesanook.EventManagement.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;

namespace Codesanook.EventManagement.Drivers {
    public class UpcomingEventsPartDriver : ContentPartDriver<UpcomingEventsPart> {
        private readonly IContentManager contentManager;
        private readonly IEventService eventService;

        public UpcomingEventsPartDriver(
            IContentManager contentManager,
            IEventService eventService
        ) {
            this.contentManager = contentManager;
            this.eventService = eventService;
        }

        protected override DriverResult Display(UpcomingEventsPart part, string displayType, dynamic shapeHelper) {

            var upcommingEvents = eventService.GetUpcommingEvents(0, part.Count);
            var eventListShape = shapeHelper.List();
            eventListShape.AddRange(
                // Transform each event part to summary shape
                upcommingEvents.Select(e => contentManager.BuildDisplay(e, "Summary"))
            );

            var  totalUpcommingEventsCount = eventService.GetUpcommingEventsCount();
            return ContentShape(
                "Parts_UpcomingEvents",
                () => shapeHelper.Parts_UpcomingEvents(
                    UpcomingEvents: eventListShape,
                    ShowViewMore: totalUpcommingEventsCount > part.Count
                )
            );
        }
    }
}
