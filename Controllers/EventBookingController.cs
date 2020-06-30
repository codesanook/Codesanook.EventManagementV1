using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Codesanook.EventManagement.Models;
using NHibernate.Linq;
using Orchard.Data;
using Orchard.Themes;

namespace Codesanook.EventManagement.Controllers {
    [Themed]
    public class EventBookingController : Controller {
        private readonly ITransactionManager transactionManager;

        public EventBookingController(ITransactionManager transactionManager) {
            this.transactionManager = transactionManager;
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

            return View(eventAttendees);
        }

        [HttpPost]
        public ActionResult Register(IList<EventAttendeeRecord> eventAttendees) {
            return View(eventAttendees);
        }

        [ActionName("register-review")]
        //[HttpPost]
        public ActionResult RegisterReview(int id) {
            // Sometimes you need to delete mapping cache
            var session = transactionManager.GetSession();
            // TODO get eager load children
            var eventBookings = session.Query<EventBookingRecord>()
                .SingleOrDefault(b => b.Id == id);
            return View(nameof(RegisterReview), eventBookings);
        }

        [ActionName("register-result")]
        //[HttpPost]
        public ActionResult RegisterResult(int id) {
            // Sometimes you need to delete mapping cache
            var session = transactionManager.GetSession();
            // TODO get eager load children
            var eventBookings = session.Query<EventBookingRecord>()
                .SingleOrDefault(b => b.Id == id);
            return View(nameof(RegisterResult), eventBookings);
        }

    }

}