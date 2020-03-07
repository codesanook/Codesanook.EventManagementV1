using Codesanook.EventManagement.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;

namespace Codesanook.EventManagement.Handlers {
    public class EventPartHandler : ContentHandler {

        public EventPartHandler(IRepository<EventPartRecord> repository) {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}
