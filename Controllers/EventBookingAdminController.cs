using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Codesanook.BasicUserProfile.Models;
using Codesanook.EventManagement.Models;
using Codesanook.EventManagement.ViewModels;
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
        private readonly ISiteService siteService;
        private readonly dynamic shapeFactory;
        private readonly IRepository<EventBookingRecord> repository;
        private readonly IContentManager contentManager;
        private readonly IShapeDisplay shapeDisplay;
        private readonly IMessageService messageService;

        public EventBookingAdminController(
            ISiteService siteService,
            IShapeFactory shapeFactory,
            IRepository<EventBookingRecord> repository,
            IContentManager contentManager,
            IShapeDisplay shapeDisplay,
            IMessageService messageService
        ) {
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
            // TODO Email to a user that booking has been approve
            var eventBooking = repository.Get(id);

            var eventPart = contentManager.Get<EventPart>(eventBooking.Id);
            var userPart = eventPart.As<UserPart>();
            var userProfilePart = eventPart.As<UserProfilePart>();

            var siteSetting = siteService.GetSiteSettings().As<SiteSettingsPart>();
            var eventBookingEmailViewModel = new EventBookingEmailViewModel {
                Event = eventPart,
                UserProfile = userProfilePart,
                SiteName = siteSetting.SiteName,
            };

            // !!! Folder lookup for shape template name works only inside "Parts" folder !!!
            var template = shapeFactory.Email_Template_SuccessfulBooking(
                EventBookingEmail: eventBookingEmailViewModel
            );

            SendEmail("Successful booking", template, userPart.Email);
            return RedirectToAction(nameof(Edit), new { id = id });
        }

        [HttpPost]
        public ActionResult Refund(int id) {
            // TODO update to Refund status 
            // TODO Email to a user that booking has been refunded
            var eventBooking = repository.Get(id);
            var eventPart = contentManager.Get<EventPart>(eventBooking.Id);
            var userPart = eventPart.As<UserPart>();
            var userProfilePart = eventPart.As<UserProfilePart>();

            var siteSetting = siteService.GetSiteSettings().As<SiteSettingsPart>();
            var eventBookingEmailViewModel = new EventBookingEmailViewModel {
                Event = eventPart,
                UserProfile = userProfilePart,
                SiteName = siteSetting.SiteName,
            };

            // !!! Folder lookup for shape template name works only inside "Parts" folder !!!
            var template = shapeFactory.Email_Template_RefundedBooking(
                EventBookingEmail: eventBookingEmailViewModel
            );

            SendEmail("Refund booking", template, userPart.Email);
            return RedirectToAction(nameof(Edit), new { id = id });
        }

        [HttpPost]
        public ActionResult InvalidPayment(int id) {
            // TODO update to InvalidPayment status 
            // TODO Email to a user that booking has invalid payment and try a new payment
            var eventBooking = repository.Get(id);
            var eventPart = contentManager.Get<EventPart>(eventBooking.Id);
            var userPart = eventPart.As<UserPart>();
            var userProfilePart = eventPart.As<UserProfilePart>();

            var siteSetting = siteService.GetSiteSettings().As<SiteSettingsPart>();
            var eventBookingEmailViewModel = new EventBookingEmailViewModel {
                Event = eventPart,
                UserProfile = userProfilePart,
                SiteName = siteSetting.SiteName,
            };

            // !!! Folder lookup for shape template name works only inside "Parts" folder !!!
            var template = shapeFactory.Email_Template_InvalidPayment(
                EventBookingEmail: eventBookingEmailViewModel
            );

            SendEmail("Invalid payment", template, userPart.Email);
            return RedirectToAction(nameof(Edit), new { id = id });
        }

        [HttpPost]
        public ActionResult Cancel(int id) {
            // TODO update to Cancel status 
            // TODO Email to a user that booking has been cancel
            // EventBookingStatus.Cancel 
            var eventBooking = repository.Get(id);
            var eventPart = contentManager.Get<EventPart>(eventBooking.Id);
            var userPart = eventPart.As<UserPart>();
            var userProfilePart = eventPart.As<UserProfilePart>();

            var siteSetting = siteService.GetSiteSettings().As<SiteSettingsPart>();
            var eventBookingEmailViewModel = new EventBookingEmailViewModel {
                Event = eventPart,
                UserProfile = userProfilePart,
                SiteName = siteSetting.SiteName,
            };

            // !!! Folder lookup for shape template name works only inside "Parts" folder !!!
            var template = shapeFactory.Email_Template_InvalidPayment(
                EventBookingEmail: eventBookingEmailViewModel
            );

            SendEmail("Booking cancelled", template, userPart.Email);
            return RedirectToAction(nameof(Edit), new { id = id });
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