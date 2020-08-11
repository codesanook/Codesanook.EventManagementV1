using System.Linq;
using System.Web.Mvc;
using Codesanook.EventManagement.Models;
using Codesanook.EventManagement.Services;
using Codesanook.EventManagement.ViewModels;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Settings;
using Orchard.UI.Admin;
using Orchard.UI.Navigation;

namespace Codesanook.EventManagement.Controllers {

    [ValidateInput(false), Admin]
    public class BankAccountAdminController : Controller {
        private readonly ISiteService siteService;
        private readonly IContentManager contentManager;
        private readonly IEventService eventService;
        private readonly dynamic shapeFactory;
        private readonly IRepository<BankAccountRecord> repository;

        protected ILogger Logger { get; set; }
        protected Localizer T { get; set; }

        public BankAccountAdminController(
            ISiteService siteService,
            IContentManager contentManager,
            IEventService eventService,
            IShapeFactory shapeFactory,
            IRepository<BankAccountRecord> repository
        ) {
            this.siteService = siteService;
            this.contentManager = contentManager;
            this.eventService = eventService;
            this.shapeFactory = shapeFactory;
            this.repository = repository;
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

            // Create shape from template in Views folder 
            // https://docs.orchardproject.net/en/latest/Documentation/Accessing-and-rendering-shapes/#naming-shapes-and-templates
            var eventShapes = events.Select(
                e => shapeFactory.BankAccountDetails()
            ).ToArray();

            var accounts = repository.Table.ToList();
            var pagerShape = shapeFactory
                    .Pager(pager)
                    .TotalItemCount(accounts.Count);

            var viewModel = new BankAccountIndexViewModel() { BankAccounts = accounts, Pager = pagerShape };
            return View(viewModel);
        }

        public ActionResult Create() {
            return View();
        }

        [HttpPost]
        public ActionResult Create([Bind(Exclude = nameof(BankAccountRecord.Id))] BankAccountRecord model) {
            if (!ModelState.IsValid) {
                return View(model);
            }

            repository.Create(model);
            return RedirectToAction(nameof(Index));
        }

        public ActionResult Edit(int id) {
            var model = repository.Get(id);
            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(int id, [Bind(Exclude = nameof(BankAccountRecord.Id))] BankAccountRecord viewModel) {
            if (!ModelState.IsValid) {
                return View(viewModel);
            }

            var model = repository.Get(id);
            model.AccountName = viewModel.AccountName;
            model.AccountNumber = viewModel.AccountNumber;
            model.BankName = viewModel.BankName;
            model.BranchName = viewModel.BranchName;
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public ActionResult Delete(int id) {
            var model = repository.Get(id);
            repository.Delete(model);
            return RedirectToAction(nameof(Index));
        }
    }
}
