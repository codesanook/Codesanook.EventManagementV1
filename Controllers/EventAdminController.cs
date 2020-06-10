using System.Linq;
using System.Web.Mvc;
using Codesanook.EventManagement.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Settings;
using Orchard.UI.Admin;
using Orchard.UI.Navigation;

namespace Codesanook.EventManagement.Controllers {

    [ValidateInput(false), Admin]
    public class EventAdminController : Controller {
        private readonly ISiteService siteService;
        private readonly IContentManager contentManager;
        private readonly IEventService eventService;
        private dynamic shapeFactory;

        protected ILogger Logger { get; set; }
        protected Localizer T { get; set; }

        public EventAdminController(
            IOrchardServices orchardServices,
            ISiteService siteService,
            IContentManager contentManager,
            IEventService eventService,
            IShapeFactory shapeFactory
        ) {
            this.siteService = siteService;
            this.contentManager = contentManager;
            this.eventService = eventService;
            this.shapeFactory = shapeFactory;

            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public ActionResult Index(PagerParameters pagerParameters) {
            var pager = new Pager(siteService.GetSiteSettings(), pagerParameters);
            var events = eventService.GetEvents(
                pager.GetStartIndex(),
                pager.PageSize,
                VersionOptions.Latest
            ).ToArray();

            var eventShapes = events.Select(
                e => contentManager.BuildDisplay(
                    e,
                    "SummaryAdmin"
                    )
            ).ToArray();

            var listShape = shapeFactory.List();
            listShape.AddRange(eventShapes);

            var totalItemCount = eventService.GetEventsCount(VersionOptions.Latest);
            var pagerShape = shapeFactory.Pager(pager).TotalItemCount(totalItemCount);

            var viewModel = shapeFactory
                .ViewModel()
                .ContentItems(listShape)
                .Pager(pagerShape);

            return View(viewModel);
        }
    }
}
