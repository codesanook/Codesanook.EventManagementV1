using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Mvc.Routes;

namespace Codesanook.EventManagement {
    public class Routes : IRouteProvider {

        public void GetRoutes(ICollection<RouteDescriptor> routes) {
            var routeDescriptors = new[] {
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/Blogs/{blogId}/Posts/Create",
                        new RouteValueDictionary {
                            {"area", "Orchard.Blogs"},
                            {"controller", "BlogPostAdmin"},
                            {"action", "Create"}
                        },
                        new RouteValueDictionary (),
                        new RouteValueDictionary {
                            {"area", "Orchard.Blogs"}
                        },
                        new MvcRouteHandler()
                    )
                },

                new RouteDescriptor {
                    Route = new Route(
                        "Admin/Event/",
                        new RouteValueDictionary {
                            {"area", "Codesanook.EventManagment"},
                            {"controller", "EventAdmin"},
                            {"action", "Edit"}
                        },
                    new RouteValueDictionary (),
                    new RouteValueDictionary {
                        {"area", "Orchard.Blogs"}
                    },
                    new MvcRouteHandler())
                },
            };

            //foreach (var routeDescriptor in routeDescriptors) {
            //    routes.Add(routeDescriptor);
            //}
        }
    }
}
