using System.Web.Mvc;

namespace Codesanook.EventManagement.Extensions {

    public static class UrlHelperExtensions {
        public static string EventEdit(this UrlHelper urlHelper) =>
            urlHelper.Action(
                "Edit",
                "EventAdmin",
                new { area = "Codesanook.EventManagement" }
            );
    }
}
