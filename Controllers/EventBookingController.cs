using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Codesanook.EventManagement.Models;
using Codesanook.EventManagement.ViewModels;
using NHibernate.Linq;
using NHibernate.Util;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Security;
using Orchard.Themes;
using Orchard.Users.Models;

namespace Codesanook.EventManagement.Controllers {
    [Themed]
    public class EventBookingController : Controller {
        private readonly ITransactionManager transactionManager;
        private readonly IContentManager contentManager;
        private readonly IAuthenticationService authenticationService;

        public EventBookingController(
            ITransactionManager transactionManager,
            IContentManager contentManager,
            IAuthenticationService authenticationService
        ) {
            this.transactionManager = transactionManager;
            this.contentManager = contentManager;
            this.authenticationService = authenticationService;
        }

        // /event-booking 
        public ActionResult Index() {
            // Get all booking with status paid/unpaid 
            var session = transactionManager.GetSession();
            var eventBookings = session
                .Query<EventBookingRecord>()
                .ToList();
            return View(eventBookings);
        }

        public ActionResult Details(int eventBookingId) {
            // Get all booking with status paid/unpaid 
            var session = transactionManager.GetSession();
            var eventBookings = session
                .Query<EventBookingRecord>()
                .ToList();
            return View(eventBookings);
        }

        public ActionResult Register(int eventId, int? eventBookingId, int? numberOfAttendees) {
            var eventAttendees = GetEventAttendees(
                eventBookingId,
                numberOfAttendees
            );

            var eventPart = contentManager.Get<EventPart>(eventId);
            var viewModel = new EventBookingRegisterViewModel() {
                Event = eventPart,
                EventAttendees = eventAttendees
            };
            return View(viewModel);
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

            // Because we set cascase all so the flow will be:  
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
            eventBooking.Status = EventBookingStatus.Comfirmed;
            eventBooking.BookingDateTimeUtc = DateTime.UtcNow;
            // We don't need to call update for connected object xxx session.Update(eventBooking);
            return RedirectToAction(nameof(RegisterResult), new { eventBookingId });
        }

        public ActionResult RegisterResult(int eventBookingId) {
            // Sometimes you need to delete mapping cache
            var viewModel = GetEventBookingViewModel(eventBookingId);
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
                PaymentConfirmationAttachementFileUrl = eventBooking.PaymentConfirmationAttachementFileUrl,
                EventAttendees = eventBooking.EventAttendees
            };

            return result;
        }

        public static string GetDateComponents(DateTime beginDate, DateTime endDate) {

            const string dateFormat = "d MMM yyyy";
            // 1 May 2020
            if (beginDate.Date == endDate.Date) {
                return beginDate.ToString(dateFormat);
            }

            // 1 - 10 May 2020
            if ((beginDate.Month == endDate.Month) && (beginDate.Year == endDate.Year)) {
                return beginDate.Day.ToString() + " - " + endDate.ToString(dateFormat);
            }

            // 1 May - 10 June 2020
            if (beginDate.Year == endDate.Year) {
                return beginDate.ToString("d MMM") + " - " + endDate.ToString(dateFormat);
            }

            // 1 May 2020 - 1 May 2021
            return beginDate.ToString(dateFormat) + " - " + endDate.ToString(dateFormat);
        }
    }
}