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

        public EventService(
            IContentManager contentManager
        ) {
            this.contentManager = contentManager;
        }

        public EventPart Get(int id) =>
            Get(id, VersionOptions.Published);

        public EventPart Get(int id, VersionOptions versionOptions) =>
            contentManager.Get<EventPart>(id, versionOptions);

        private IContentQuery<ContentItem, CommonPartRecord> GetEventQuery(VersionOptions versionOptions) =>
            contentManager.Query(versionOptions, "Event")
            .Join<CommonPartRecord>()
            .OrderByDescending(cr => cr.CreatedUtc);

        public IReadOnlyCollection<EventPart> Get(int skip, int count, VersionOptions versionOptions) =>
            GetEventQuery(versionOptions)
            .Slice(skip, count)
            .ToList()// TODO need to optimize because it pull all event to client
            .Select(ci => ci.As<EventPart>())
            .ToArray();

        public int GetEventCount(VersionOptions versionOptions) =>
            contentManager.Query(versionOptions, "Event").Count();
    }
}
