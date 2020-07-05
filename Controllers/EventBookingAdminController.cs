using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Codesanook.EventManagement.Models;
using NHibernate.Linq;
using Orchard.Data;
using Orchard.UI.Admin;

namespace Codesanook.EventManagement.Controllers {
    [Admin]
    public class EventBookingAdminController : Controller {
        private readonly ITransactionManager transactionManager;

        public EventBookingAdminController(ITransactionManager transactionManager) {
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
    }
}