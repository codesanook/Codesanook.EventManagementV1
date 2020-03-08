using System.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents;
using Orchard.Localization;
using Orchard.Mvc.Html;
using Orchard.UI.Navigation;

namespace Codesanook.EventManagement {
    public class AdminMenu : INavigationProvider {

        private readonly IContentManager contentManager;

        public AdminMenu(IContentManager contentManager) {
            this.contentManager = contentManager;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

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

            var contentItem = contentManager.New("Event");
            var contentItemMetadata = contentManager.GetItemMetadata(contentItem);
            menu.Add(
                T("New event"),
                "1.1",
                item => item.Action(
                    contentItemMetadata.CreateRouteValues["Action"] as string,
                    contentItemMetadata.CreateRouteValues["Controller"] as string,
                    contentItemMetadata.CreateRouteValues
                )
                // Apply "CreateContent" permission for the content type
                .Permission(
                    DynamicPermissions.CreateDynamicPermission(
                        DynamicPermissions.PermissionTemplates[Permissions.CreateContent.Name],
                        contentItem.TypeDefinition
                    )
                )
            );

        }
    }
}
