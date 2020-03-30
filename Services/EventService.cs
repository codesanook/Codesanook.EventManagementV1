using System;
using System.Collections.Generic;
using System.Linq;
using Codesanook.EventManagement.Models;
using Codesanook.EventManagement.Services;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;

namespace Codesanook.EventManagement.Blogs.Services {
    public class EventService : IEventService {
        private readonly IContentManager contentManager;

        public EventService(IContentManager contentManager) {
            this.contentManager = contentManager;
        }

        public EventPart Get(int id) =>
            Get(id, VersionOptions.Published);

        public EventPart Get(int id, VersionOptions versionOptions) =>
            contentManager.Get<EventPart>(id, versionOptions);

        public IReadOnlyCollection<EventPart> GetEvents(int skip, int count, VersionOptions versionOptions) =>
            GetEventQuery(versionOptions)
            .Slice(skip, count) // Slice generate paging 
            .ToList()// TODO need to optimize because it pull all event to client
            .Select(ci => ci.As<EventPart>())
            .ToArray();

        public IReadOnlyCollection<EventPart> GetUpcommingEvents(int skip, int count) {
            var events = contentManager.Query(VersionOptions.Published, "Event") // Event type
                .Join<EventPartRecord>()
                .Where(e => e.BeginDateTimeUtc > DateTime.UtcNow) // Future event
                .OrderByDescending(e => e.BeginDateTimeUtc)
                .Slice(skip, count)
                .ToList() // Execute SQL
                .Select(e => e.As<EventPart>())
                .ToList();
            return events;
        }

        private IContentQuery<ContentItem, CommonPartRecord> GetEventQuery(VersionOptions versionOptions) =>
            contentManager.Query(versionOptions, "Event")
            .Join<CommonPartRecord>()
            .OrderByDescending(cr => cr.CreatedUtc);

        public int GetEventsCount(VersionOptions versionOptions) =>
            contentManager.Query(versionOptions, "Event").Count();

        public int GetUpcommingEventsCount() {
            /// Gets the latest published version.
            var count = contentManager.Query(VersionOptions.Published, "Event") // Event type
                .Join<EventPartRecord>()
                .Where(e => e.BeginDateTimeUtc > DateTime.UtcNow) // Future event
                .Count();
            return count;
        }
    }
}
