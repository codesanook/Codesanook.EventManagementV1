using System.Collections.Generic;
using Codesanook.EventManagement.Models;
using Orchard;
using Orchard.ContentManagement;

namespace Codesanook.EventManagement.Services {
    public interface IEventService : IDependency {
        IReadOnlyCollection<EventPart> GetEvents(int skip, int count, VersionOptions versionOptions);
        IReadOnlyCollection<EventPart> GetUpcommingEvents(int skip, int count); 
        int GetEventsCount(VersionOptions versionOptions);
        int GetUpcommingEventsCount();
    }
}
