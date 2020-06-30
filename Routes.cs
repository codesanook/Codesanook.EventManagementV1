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
                Name = "EventBooking",
                Priority = 100,
                Route = new Route(
                    url:"event-booking/{action}/{id}",
                    defaults: new RouteValueDictionary {
                        { "controller", "EventBooking" }, // no controller suffix
                        { "action", "Index" },
                        { "id", UrlParameter.Optional },
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
