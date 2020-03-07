using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Codesanook.EventManagement.Models;
using Orchard;
using Orchard.ContentManagement;

namespace Codesanook.EventManagement.Services {
    public interface IEventService : IDependency {
        IReadOnlyCollection<EventPart> Get(int skip, int count, VersionOptions versionOptions);
        int GetEventCount(VersionOptions versionOptions);
    }
}
