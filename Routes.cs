using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Mvc.Routes;

namespace Codesanook.EventManagement {
    public class Routes : IRouteProvider {

        public void GetRoutes(ICollection<RouteDescriptor> routes) {
            foreach (var routeDescriptor in GetRoutes()) {
                routes.Add(routeDescriptor);
            }
        }

        public IEnumerable<RouteDescriptor> GetRoutes() => new[] {

            new RouteDescriptor {
                Name = "EventBookingRegister",
                Priority = 100,
                Route = new Route(
                    url:"EventBooking/Register/{eventId}/{eventBookingId}",
                    defaults: new RouteValueDictionary {
                        { "controller", "EventBooking" }, // no controller suffix
                        { "action", "Register" },
                        { "eventBookingId", UrlParameter.Optional },
                    },
                    constraints: new RouteValueDictionary(),
                    dataTokens: new RouteValueDictionary {
                        { "area", "Codesanook.EventManagement" }
                    },
                    routeHandler: new MvcRouteHandler()
                )
            },
            new RouteDescriptor {
                Name = "EventBookingAction",
                Priority = 100,
                Route = new Route(
                    url:"EventBooking/{action}/{eventBookingId}",
                    defaults: new RouteValueDictionary {
                        { "controller", "EventBooking" }, // no controller suffix
                        { "action", "Index" },
                        { "eventBookingId", UrlParameter.Optional },
                    },
                    constraints: new RouteValueDictionary(),
                    dataTokens: new RouteValueDictionary {
                        { "area", "Codesanook.EventManagement" }
                    },
                    routeHandler: new MvcRouteHandler()
                )
            }
        };
    }
}
