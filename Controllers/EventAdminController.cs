using System.Dynamic;
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
    public class EventAdminController : Controller, IUpdateModel {
        private readonly IOrchardServices orchardServices;
        private readonly ISiteService siteService;
        private readonly IContentManager contentManager;
        private readonly IEventService eventService;

        public EventAdminController(
            IOrchardServices orchardServices,
            ISiteService siteService,
            IContentManager contentManager,
            IEventService eventService,
            IShapeFactory shapeFactory
        ) {
            T = NullLocalizer.Instance;
            this.orchardServices = orchardServices;
            this.siteService = siteService;
            this.contentManager = contentManager;
            this.eventService = eventService;
            Shape = shapeFactory;
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        dynamic Shape { get; set; }
        protected ILogger Logger { get; set; }
        public Localizer T { get; set; }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }

        bool IUpdateModel.TryUpdateModel<TModel>(
            TModel model,
            string prefix,
            string[] includeProperties,
            string[] excludeProperties
        ) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        //public ActionResult Create() {
        //    var eventItem = orchardServices.ContentManager.New<EventPart>("Event");
        //    // how do we link for content blogPost.BlogPart = blog;

        //    //if (!Services.Authorizer.Authorize(Permissions.EditBlogPost, blogPost, T("Not allowed to create blog post")))
        //    //    return new HttpUnauthorizedResult();

        //    var model = orchardServices.ContentManager.BuildEditor(eventItem);
        //    return View(model);
        //}

        //[HttpPost]
        //public ActionResult Create(bool publish = false) {
        //    var eventPart = orchardServices.ContentManager.New<EventPart>("Event");

        //    orchardServices.ContentManager.Create(eventPart, VersionOptions.Draft);
        //    var model = orchardServices.ContentManager.UpdateEditor(eventPart, this);

        //    if (!ModelState.IsValid) {
        //        orchardServices.TransactionManager.Cancel();
        //        return View(model);
        //    }

        //    if (publish) {
        //        orchardServices.ContentManager.Publish(eventPart.ContentItem);
        //    }

        //    orchardServices.Notifier.Success(T($"Your {eventPart.TypeDefinition.DisplayName} has been created."));
        //    return Redirect(Url.EventEdit());
        //}

        public ActionResult Index(PagerParameters pagerParameters) {
            var pager = new Pager(siteService.GetSiteSettings(), pagerParameters);
            var events = eventService.Get(pager.GetStartIndex(), pager.PageSize, VersionOptions.Latest).ToArray();
            var eventShapes = events.Select(e => contentManager.BuildDisplay(e, "SummaryAdmin")).ToArray();

            var listShape = Shape.List();
            listShape.AddRange(eventShapes);

            var totalItemCount = eventService.GetEventCount(VersionOptions.Latest);
            var pagerShape = Shape.Pager(pager).TotalItemCount(totalItemCount);

            var viewModel = Shape.ViewModel()
                .ContentItems(listShape)
                .Pager(pager);

            return View(viewModel);
        }

    }
}
