using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Web.Mvc;
using Codesanook.EventManagement.Models;
using Codesanook.EventManagement.Services;
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

        public ActionResult Details(int id) {
            // Get all booking with status paid/unpaid 
            var session = transactionManager.GetSession();
            var eventBookings = session
                .Query<EventBookingRecord>()
                .ToList();
            return View(eventBookings);
        }

        public ActionResult Register(int id, int numberOfAttendees) {
            // https://haacked.com/archive/2008/10/23/model-binding-to-a-list.aspx/
            var eventAttendees = Enumerable.Range(0, numberOfAttendees)
                .Select(_ => new EventAttendeeRecord())
                .ToList();

            var eventPart = contentManager.Get<EventPart>(id);
            var viewModel = new EventBookingRegisterViewModel() {
                Event = eventPart,
                EventAttendees = eventAttendees
            };
            return View(viewModel);
        }

        [HttpPost]
        public ActionResult Register(int id, IList<EventAttendeeRecord> eventAttendees) {
            // Form validation

            var eventPart = contentManager.Get<EventPart>(id);

            if (ModelState.IsValid) {
                // Save with NHibernate session
                var session = transactionManager.GetSession();
                var user = authenticationService.GetAuthenticatedUser();

                var eventBooking = new EventBookingRecord() {
                    Event = eventPart.Record,
                    User = user.As<UserPart>().Record,
                    BookingDateTimeUtc = DateTime.Now,
                    Status = EventBookingStatus.Unpaid,
                };
                eventAttendees.ForEach(a => eventBooking.AddEventAttendee(a));

                session.Save(eventBooking);
                // Because we set cascase all so the flow will be:  
                // Insert a new EventBookingRecord
                // Insert a new EventAttendeeRecord
                return RedirectToAction(nameof(RegisterConfirm), new { id });
            }

            var viewModel = new EventBookingRegisterViewModel() {
                Event = eventPart,
                EventAttendees = eventAttendees
            };
            return View(viewModel);
        }

        public ActionResult RegisterConfirm(int id) {
            // Sometimes you need to delete mapping cache
            var session = transactionManager.GetSession();
            // TODO get eager load children
            var eventBookings = session.Query<EventBookingRecord>()
                .SingleOrDefault(b => b.Id == id);
            return View(eventBookings);
        }

        [HttpPost]
        public ActionResult RegisterConfirm(int id, FormCollection form) {
            // Sometimes you need to delete mapping cache
            var session = transactionManager.GetSession();
            // TODO get eager load children
            var eventBookings = session.Query<EventBookingRecord>()
                .SingleOrDefault(b => b.Id == id);
            return RedirectToAction(nameof(RegisterResult), new { id = id });
        }


        public ActionResult RegisterResult(int id) {
            // Sometimes you need to delete mapping cache
            var session = transactionManager.GetSession();
            // TODO get eager load children
            var eventBookings = session.Query<EventBookingRecord>()
                .SingleOrDefault(b => b.Id == id);
            return View(eventBookings);
        }

    }

}