using System.Collections.Generic;
using System.Linq;
using Codesanook.EventManagement.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Common.Models;
using Orchard.Mvc;
using Orchard.Security;
using Orchard.Users.Models;

namespace Codesanook.EventManagement.Drivers {
    public class EventListPartDriver : ContentPartDriver<EventListPart>
    {
        private readonly IContentManager contentManager;
        private readonly IAuthenticationService auth;
        private readonly IHttpContextAccessor httpContextAccessor;

        public EventListPartDriver(
            IContentManager contentManager,
            IAuthenticationService auth,
            IHttpContextAccessor httpContextAccessor
        )
        {
            this.contentManager = contentManager;
            this.auth = auth;
            this.httpContextAccessor = httpContextAccessor;
        }

        protected override string Prefix => nameof(EventListPart);

        protected override DriverResult Display(
            EventListPart part,
            string displayType,
            dynamic shapeHelper
        )
        {
            var contentItemId = part.ContentItem.Id;
            var user = auth.GetAuthenticatedUser();

            if (displayType == "Detail")
            {
                var eventList = new List<EventItemViewModel>();
                var events = contentManager.HqlQuery().ForType("Event")
                     .Where(alias => alias.ContentPartRecord<EventPartRecord>(), s => s.Eq("ContentItemId", contentItemId))
                     .OrderBy(alias => alias.ContentPartRecord<CommonPartRecord>(), order => order.Desc("CreatedUtc"))
                     .List()
                     .ToList();

                if (events.Any())
                {
                    var userIds = events.Select(c => c.As<CommonPart>().Owner.Id).ToArray();
                    var users = contentManager.HqlQuery().ForType("User")
                       .Where(alias => alias.ContentPartRecord<UserPartRecord>(),
                       q => q.In("Id", userIds))
                       .List()
                       .ToList();

                    eventList = (from c in events
                                   join u in users
                                   on (c.As<CommonPart>().Owner.Id) equals u.Id
                                   select new EventItemViewModel()
                                   {
                                       Event = c,
                                       User = u
                                   }).ToList();
                }

                var newEvent = contentManager.New("Event");
                var eventPart = newEvent.As<EventPart>();
                var httpContext = httpContextAccessor.Current();
                var sessionKey = "tempeventPart";
                var tempeventPart = httpContext.Session[sessionKey] as EventPart;
                if (tempeventPart != null)
                {
                    eventPart.Details = tempeventPart.Details;
                    httpContext.Session.Remove(sessionKey);
                }

                var eventShape = contentManager.BuildEditor(newEvent);
                var containerShape = ContentShape("Parts_eventContainer",
                      () => shapeHelper.Parts_eventContainer(
                          eventShape: eventShape,
                          eventList: eventList));

                return Combined(containerShape);
            }
            else
            {
                var summaryShape = ContentShape("Parts_eventSummary",
                    () => shapeHelper.Parts_Parts_eventSummary());
                return Combined(summaryShape);
            }
        }
    }
}
