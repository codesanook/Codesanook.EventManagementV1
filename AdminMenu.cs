using System.Linq;
using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Navigation;

namespace Codesanook.EventManagement {
    public class AdminMenu : INavigationProvider {

        public Localizer T { get; set; }

        public AdminMenu() {
            T = NullLocalizer.Instance;
        }

        // For admin menu
        public string MenuName => "admin";

        public void GetNavigation(NavigationBuilder builder) {
            builder
                .AddImageSet("calendar")
                .Add(T("Event"), "1.5", BuildChildMenu);
        }

        private void BuildChildMenu(NavigationItemBuilder menu) {
            menu.LinkToFirstChild(false);

            menu.Add(
                T("All events"),
                "1.0",
                item => item.Action(
                    "Index",
                    "EventAdmin",
                    new { area = "Codesanook.EventManagement" }
                )
             );

            //menu.Add(
            //    T("New event"), "1.1",
            //    item => item.Action("Create2", "EventAdmin", new { area = "Codesanook.EventManagement" })
            // );
        }
    }
}
