using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Amazon.S3;
using Amazon.S3.Model;
using Codesanook.AmazonS3.Models;
using Codesanook.EventManagement.Models;
using Codesanook.EventManagement.ViewModels;
using Codesanook.BasicUserProfile.Models;
using NHibernate.Linq;
using NHibernate.Util;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.DisplayManagement;
using Orchard.Email.Services;
using Orchard.Localization;
using Orchard.Messaging.Services;
using Orchard.Security;
using Orchard.Settings;
using Orchard.Themes;
using Orchard.Users.Models;

namespace Codesanook.EventManagement.Controllers {
    [Themed]
    public class EventBookingController : Controller {
        private readonly ITransactionManager transactionManager;
        private readonly IContentManager contentManager;
        private readonly IAuthenticationService authenticationService;
        private readonly dynamic shapeFactory;
        private readonly ISiteService siteService;
        private readonly IAmazonS3 s3Client;
        private readonly IShapeDisplay shapeDisplay;
        private readonly IMessageService messageService;

        public Localizer T { get; set; }

        public EventBookingController(
            ITransactionManager transactionManager,
            IContentManager contentManager,
            IAuthenticationService authenticationService,
            IShapeFactory shapeFactory,
            ISiteService siteService,
            IAmazonS3 s3Client,
            IShapeDisplay shapeDisplay,
            IMessageService messageService
        ) {
            this.transactionManager = transactionManager;
            this.contentManager = contentManager;
            this.authenticationService = authenticationService;
            this.shapeFactory = shapeFactory;
            this.siteService = siteService;
            this.s3Client = s3Client;
            this.shapeDisplay = shapeDisplay;
            this.messageService = messageService;
            T = NullLocalizer.Instance;
        }

        // /event-booking 
        public ActionResult Index() {
            // Get all booking with status paid/unpaid 
            var session = transactionManager.GetSession();
            var user = authenticationService.GetAuthenticatedUser();
            if (user != null) {
                var userRecord = user.As<UserPart>().Record;
                var eventBookings = session
                    .Query<EventBookingRecord>()
                    .Where(x => x.User == userRecord)
                    .ToList();

                List<EventBookingViewModel> viewModels = new List<EventBookingViewModel>();
                foreach (var eb in eventBookings) {
                    viewModels.Add(GetEventBookingViewModel(eb.Id)); ;
                }
                return View(viewModels);
            }
            else {
                return Redirect("~/Users/Account/AccessDenied?ReturnUrl=%2FEventBooking%2F");
            }
        }

        public ActionResult Details(int eventBookingId) {

            var session = transactionManager.GetSession();
            var eventBooking = session.Get<EventBookingRecord>(eventBookingId);
            var eventPart = contentManager.Get<EventPart>(eventBooking.Event.Id);
            var userPart = contentManager.Get<UserPart>(eventBooking.User.Id);

            // Build display for event content item
            var eventShape = contentManager.BuildDisplay(eventPart.ContentItem);

            var viewModel = shapeFactory.ViewModel(
                EventShape: eventShape,
                EventBooking: eventBooking
            );
            return View(viewModel);
        }

        [HttpPost]
        public async Task<ActionResult> InformPayment(
            int eventBookingId,
            HttpPostedFileBase postedFile
        ) {

            var session = transactionManager.GetSession();
            var eventBooking = session.Get<EventBookingRecord>(eventBookingId);
            var eventPart = contentManager.Get<EventPart>(eventBooking.Event.Id);
            var userPart = contentManager.Get<UserPart>(eventBooking.User.Id);

            // ToDo upload a file with Amazon S3 IAmazonS3
            var s3Setting = siteService.GetSiteSettings().As<AwsS3SettingPart>();

            var dto = new DateTimeOffset(DateTime.UtcNow, TimeSpan.Zero);
            var timestamp = dto.ToUnixTimeSeconds();
            var fileExtension = Path.GetExtension(postedFile.FileName);
            var fileKey =
                $"payment-confirmation/event-id-{eventPart.Id}/booking-id-{eventBooking.Id}-{timestamp}{fileExtension}";

            using (var stream = postedFile.InputStream) {
                var request = new PutObjectRequest() {
                    BucketName = s3Setting.AwsS3BucketName,
                    Key = fileKey,
                    InputStream = stream,
                    CannedACL = S3CannedACL.Private,
                    ContentType = postedFile.ContentType,
                };

                var response = await s3Client.PutObjectAsync(request);

                eventBooking.PaymentConfirmationAttachementFileKey = $"{fileKey}";
                eventBooking.Status = EventBookingStatus.VerifyingPayment;
            }

            return RedirectToAction(nameof(Details), new { eventBookingId });
        }


        public ActionResult Register(int eventId, int? eventBookingId, int? numberOfAttendees) {
            var eventAttendees = GetEventAttendees(
                eventBookingId,
                numberOfAttendees
            );
            var user = authenticationService.GetAuthenticatedUser();
            if (user != null) {
                var eventPart = contentManager.Get<EventPart>(eventId);
                var viewModel = new EventBookingRegisterViewModel() {
                    Event = eventPart,
                    EventAttendees = eventAttendees
                };
                return View(viewModel);
            }
            else {
                return Redirect("~/Users/Account/AccessDenied?ReturnUrl=" + Request.Url.ToString());
            }
        }

        [HttpPost]
        public ActionResult Register(
            int eventId,
            int? eventBookingId,
            IList<EventAttendeeRecord> eventAttendees
        ) {
            var eventPart = contentManager.Get<EventPart>(eventId);

            // Early return
            if (!ModelState.IsValid) {
                var viewModel = new EventBookingRegisterViewModel() {
                    Event = eventPart,
                    EventAttendees = eventAttendees
                };
                return View(viewModel);
            }

            var session = transactionManager.GetSession();
            if (eventBookingId.HasValue) {
                // Update only EventAttendee directly.
                // They are disconnected objects, no objects in a session so update is safe  
                eventAttendees.ForEach(a => session.Update(a));
            }
            else {
                // Save with NHibernate session
                var user = authenticationService.GetAuthenticatedUser();
                var eventBooking = new EventBookingRecord() {
                    Event = eventPart.Record,
                    User = user.As<UserPart>().Record,
                    Status = EventBookingStatus.Uncomfirmed,
                };
                // Add each attendee to event booking
                eventAttendees.ForEach(a => eventBooking.AddEventAttendee(a));
                session.Save(eventBooking);
                // Set a saved event booking Id
                eventBookingId = eventBooking.Id;
            }

            // Because we set cascade all so the flow will be:  
            // Insert a new EventBookingRecord
            // Insert a new EventAttendeeRecord
            return RedirectToAction(
                nameof(RegisterConfirm),
                new { eventBookingId = eventBookingId.Value }
            );
        }

        public ActionResult RegisterConfirm(int eventBookingId) {
            var viewModel = GetEventBookingViewModel(eventBookingId);
            return View(viewModel);
        }

        [HttpPost]
        public ActionResult RegisterConfirm(int eventBookingId, FormCollection form) {
            var session = transactionManager.GetSession();
            var eventBooking = session.Get<EventBookingRecord>(eventBookingId);
            eventBooking.Status = EventBookingStatus.WatingForPayment;
            eventBooking.BookingDateTimeUtc = DateTime.UtcNow;

            return RedirectToAction(nameof(RegisterResult), new { eventBookingId });
        }

        private void SendConfirmedBookingEmail(EventBookingRecord eventBooking) {
            var eventPart = contentManager.Get<EventPart>(eventBooking.Event.Id);
            var user = authenticationService.GetAuthenticatedUser();

            // Send an email
            // !!! Folder lookup works only "Parts" folder !!!
            ConfirmedBookingViewModel template = shapeFactory.Email_Template_ConfirmedBooking(
                typeof(ConfirmedBookingViewModel)
            );
            template.Event = eventPart;
            template.UserProfile = user.As<UserProfilePart>();

            // Render a shape
            var bodyHtml = shapeDisplay.Display(template);
            var parameters = new Dictionary<string, object>
            {
                { "Subject", T("Booking confirmed").Text },
                { "Body", bodyHtml }, // Already transformed to HTML with shapeDisplay.Display
                { "Recipients",  user.Email } // CSV for multiple emails
            };

            // The underlying class is SmtpMessageChannel
            // It handles exception internally and not throw up to a caller.
            messageService.Send(
                DefaultEmailMessageChannelSelector.ChannelName,
                parameters
            );
        }

        public ActionResult RegisterResult(int eventBookingId) {
            var session = transactionManager.GetSession();
            var eventbooking = session.Get<EventBookingRecord>(eventBookingId);
            var eventPart = contentManager.Get<EventPart>(eventbooking.Event.Id);
            var accounts = session.Query<BankAccountRecord>().ToList();

            var viewModel = new EventBookingResultViewModel() {
                Event = eventPart,
                BankAccounts = accounts
            };
            return View(viewModel);
        }

        private IList<EventAttendeeRecord> GetEventAttendees(int? eventBookingId, int? numberOfAttendees) {
            if (eventBookingId.HasValue) {
                var session = transactionManager.GetSession();
                var eventBookings = session
                    .Query<EventBookingRecord>()
                    .SingleOrDefault(b => b.Id == eventBookingId);
                return eventBookings.EventAttendees;
            }

            // https://haacked.com/archive/2008/10/23/model-binding-to-a-list.aspx/
            var eventAttendees = Enumerable.Range(0, numberOfAttendees.Value)
                .Select(_ => new EventAttendeeRecord())
                .ToList();
            return eventAttendees;
        }

        private EventBookingViewModel GetEventBookingViewModel(int eventBookingId) {
            var session = transactionManager.GetSession();
            var eventBooking = session.Get<EventBookingRecord>(eventBookingId);
            var eventPart = contentManager.Get<EventPart>(eventBooking.Event.Id);
            var userPart = contentManager.Get<UserPart>(eventBooking.User.Id);

            var result = new EventBookingViewModel() {
                Id = eventBooking.Id,
                Event = eventPart,
                User = userPart,
                BookingDateTimeUtc = eventBooking.BookingDateTimeUtc,
                Status = eventBooking.Status,
                PaidDateTimeUtc = eventBooking.PaidDateTimeUtc,
                PaymentConfirmationAttachementFileUrl = eventBooking.PaymentConfirmationAttachementFileKey,
                EventAttendees = eventBooking.EventAttendees
            };

            return result;
        }
    }
}