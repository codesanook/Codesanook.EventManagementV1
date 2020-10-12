using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Codesanook.EventManagement.Models;
using Codesanook.EventManagement.ViewModels;
using Codesanook.Users.Models;
using NHibernate.Linq;
using Orchard.ContentManagement;
using Orchard.Core.Settings.Models;
using Orchard.Data;
using Orchard.DisplayManagement;
using Orchard.Email.Services;
using Orchard.Messaging.Services;
using Orchard.Settings;
using Orchard.UI.Admin;
using Orchard.UI.Navigation;
using Orchard.Users.Models;

namespace Codesanook.EventManagement.Controllers {
    [Admin]
    public class EventBookingAdminController : Controller {
        private readonly ITransactionManager transactionManager;
        private readonly ISiteService siteService;
        private readonly dynamic shapeFactory;
        private readonly IRepository<EventBookingRecord> repository;
        private readonly IContentManager contentManager;
        private readonly IShapeDisplay shapeDisplay;
        private readonly IMessageService messageService;

        public EventBookingAdminController(
             ITransactionManager transactionManager,
            ISiteService siteService,
            IShapeFactory shapeFactory,
            IRepository<EventBookingRecord> repository,
            IContentManager contentManager,
            IShapeDisplay shapeDisplay,
            IMessageService messageService
        ) {
            this.transactionManager = transactionManager;
            this.siteService = siteService;
            this.shapeFactory = shapeFactory;
            this.repository = repository;
            this.contentManager = contentManager;
            this.shapeDisplay = shapeDisplay;
            this.messageService = messageService;
        }

        public ActionResult Index(PagerParameters pagerParameters) {
            var pager = new Pager(siteService.GetSiteSettings(), pagerParameters);
            // Get all booking with status paid/unpaid 
            var eventBookings = repository
                .Table
                .ToList();

            var pagerShape = shapeFactory
                .Pager(pager)
                .TotalItemCount(eventBookings.Count);

            ViewBag.Pager = pagerShape;
            return View(eventBookings);
        }

        public ActionResult Edit(int id) {
            var eventBooking = repository.Get(id);
            return View(eventBooking);
        }

        [HttpPost]
        //[ValidateAntiForgeryTokenOrchard(enabled:false)]
        public ActionResult Approve(int id) {
            // TODO update to Successful status
            // TODO Email to a user that booking has been approve\
            var session = transactionManager.GetSession();
            var eventBooking = session.Get<EventBookingRecord>(id);
            var eventBookingEmailViewModel = SetEventBookingEmailViewModel(eventBooking);
            var userPart = contentManager.Get<UserPart>(eventBooking.User.Id);

            // !!! Folder lookup for shape template name works only inside "Parts" folder !!!
            var template = shapeFactory.Email_Template_SuccessfulBooking(
                EventBookingEmail: eventBookingEmailViewModel
            );

            SendEmail("Successful booking", template, userPart.Email);

            eventBooking.Status = EventBookingStatus.Successful;
            session.Save(eventBooking);

            return RedirectToAction(nameof(Edit), new { id });
        }

        [HttpPost]
        public ActionResult Refund(int id) {
            // TODO update to Refund status 
            // TODO Email to a user that booking has been refunded
            var session = transactionManager.GetSession();
            var eventBooking = session.Get<EventBookingRecord>(id);
            var eventBookingEmailViewModel = SetEventBookingEmailViewModel(eventBooking);
            var userPart = contentManager.Get<UserPart>(eventBooking.User.Id);

            // !!! Folder lookup for shape template name works only inside "Parts" folder !!!
            var template = shapeFactory.Email_Template_RefundedBooking(
                EventBookingEmail: eventBookingEmailViewModel
            );

            SendEmail("Refund booking", template, userPart.Email);
            eventBooking.Status = EventBookingStatus.Refunded;
            session.Save(eventBooking);

            return RedirectToAction(nameof(Edit), new { id });
        }

        [HttpPost]
        public ActionResult InvalidPayment(int id) {
            // TODO update to InvalidPayment status 
            // TODO Email to a user that booking has invalid payment and try a new payment
            var session = transactionManager.GetSession();
            var eventBooking = session.Get<EventBookingRecord>(id);
            var eventBookingEmailViewModel = SetEventBookingEmailViewModel(eventBooking);
            var userPart = contentManager.Get<UserPart>(eventBooking.User.Id);

            dynamic MyDynamic = new System.Dynamic.ExpandoObject();
            MyDynamic.InvalidPaymentReason = "";
            eventBookingEmailViewModel.AdditionalInformation = MyDynamic;

            var accounts = session.Query<BankAccountRecord>().ToList();
            eventBookingEmailViewModel.BankAccounts = accounts;

            // !!! Folder lookup for shape template name works only inside "Parts" folder !!!
            var template = shapeFactory.Email_Template_InvalidPayment(
                EventBookingEmail: eventBookingEmailViewModel
            );

            SendEmail("Invalid payment", template, userPart.Email);
            eventBooking.Status = EventBookingStatus.InvalidPayment;
            session.Save(eventBooking);

            return RedirectToAction(nameof(Edit), new { id });
        }

        [HttpPost]
        public ActionResult Cancel(int id) {
            // TODO update to Cancel status 
            // TODO Email to a user that booking has been cancel
            // EventBookingStatus.Cancel 
            var session = transactionManager.GetSession();
            var eventBooking = session.Get<EventBookingRecord>(id);
            var eventBookingEmailViewModel = SetEventBookingEmailViewModel(eventBooking);
            var userPart = contentManager.Get<UserPart>(eventBooking.User.Id);
            // !!! Folder lookup for shape template name works only inside "Parts" folder !!!
            var template = shapeFactory.Email_Template_CancelledBooking(
                EventBookingEmail: eventBookingEmailViewModel
            );

            SendEmail("Booking cancelled", template, userPart.Email);
            eventBooking.Status = EventBookingStatus.Cancelled;
            session.Save(eventBooking);

            return RedirectToAction(nameof(Edit), new { id });
        }


        private EventBookingEmailViewModel SetEventBookingEmailViewModel(EventBookingRecord eventBooking) {

            var eventPart = contentManager.Get<EventPart>(eventBooking.Event.Id);
            var userPart = contentManager.Get<UserPart>(eventBooking.User.Id);
            var userProfilePart = userPart.As<BasicUserProfilePart>();

            var siteSetting = siteService.GetSiteSettings().As<SiteSettingsPart>();
            var eventBookingEmailViewModel = new EventBookingEmailViewModel {
                Event = eventPart,
                UserProfile = userProfilePart,
                SiteName = siteSetting.SiteName,
            };

            return eventBookingEmailViewModel;
        }

        private void SendEmail(string subJect, dynamic template, string recipients) {
            // Render a shape
            var bodyHtml = shapeDisplay.Display(template);
            var parameters = new Dictionary<string, object>{
                    { "Subject", subJect },
                    { "Body", bodyHtml }, // The body is transformed to HTML by shapeDisplay.Display
                    { "Recipients",  recipients } // CSV for multiple email
                };

            // The underlying class to send an email is SmtpMessageChannel
            // It handles exception internally and not throw up to a caller.
            messageService.Send(
                DefaultEmailMessageChannelSelector.ChannelName,
                parameters
            );
        }
    }
}