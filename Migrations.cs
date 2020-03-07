using System;
using Codesanook.EventManagement.Models;
using Orchard.Autoroute.Models;
using Orchard.Autoroute.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Common.Models;
using Orchard.Core.Contents.Extensions;
using Orchard.Core.Navigation.Models;
using Orchard.Core.Navigation.Services;
using Orchard.Core.Navigation.Settings;
using Orchard.Core.Title.Models;
using Orchard.Data.Migration;
using Orchard.Projections.Models;
using Orchard.Security;
using Orchard.UI.Navigation;
using Orchard.Utility;

namespace Codesanook.EventManagement {
    public class Migrations : DataMigrationImpl {
        private readonly IAuthenticationService authenticationService;
        private readonly IContentManager contentManager;
        private readonly IMenuService menuService;
        private readonly INavigationManager navigationManager;
        private readonly Lazy<IAutorouteService> autorouteService;

        public Migrations(
            IAuthenticationService authenticationService,
            IContentManager contentManager,
            IMenuService menuService,
            INavigationManager navigationManager,
            Lazy<IAutorouteService> autorouteService
        ) {
            this.authenticationService = authenticationService;
            this.contentManager = contentManager;
            this.menuService = menuService;
            this.navigationManager = navigationManager;
            this.autorouteService = autorouteService;
        }

        public int Create() {
            SchemaBuilder.CreateTable(
                nameof(EventPartRecord),
                table => table
                    .ContentPartRecord()
                    .Column<string>(nameof(EventPartRecord.Location), c => c.WithLength(1024))
                    .Column<DateTime>(nameof(EventPartRecord.BeginDateTimeUtc))
                    .Column<DateTime>(nameof(EventPartRecord.EndDateTimeUtc))
                    .Column<int>(nameof(EventPartRecord.MaxAttendees))
            );

            // Prepare content part
            ContentDefinitionManager.AlterPartDefinition(
                nameof(EventPart),
                builder => builder
                    .Attachable()
                    .WithDescription("Provides event details")
            );

            // Prepare content type
            ContentDefinitionManager.AlterTypeDefinition(
                "Event",
                builder => builder
                    .WithPart("EventPart")
                    .WithPart("TitlePart")
                    .WithPart("BodyPart")
                    .WithPart(
                        "CommonPart",
                        config => config
                            .WithSetting("DateEditorSettings.ShowDateEditor", "False")
                            .WithSetting("OwnerEditorSettings.ShowOwnerEditor", "False")

                    )
                    .WithPart("PublishLaterPart")
                    .WithPart("AutoroutePart", config => config
                        .WithSetting("AutorouteSettings.AllowCustomPattern", "False")
                        .WithSetting("AutorouteSettings.AutomaticAdjustmentOnEdit", "False")
                        .WithSetting("AutorouteSettings.PatternDefinitions",
                            "[{'Name': 'Event details by id', 'Pattern': 'events/{Content.Id}', 'Description': 'events/event-id'}]"
                        ))
                    .Draftable()
            );

            CreateProjectionItemForEventPart();
            return 1;
        }

        private void CreateProjectionItemForEventPart() {
            /// New The content item is not yet persisted!
            /// Creates (persists) a new content item
            var projectionPage = contentManager.Create("ProjectionPage");

            var commonPart = projectionPage.As<CommonPart>();
            var owner = authenticationService.GetAuthenticatedUser();
            commonPart.Owner = owner;

            var titlePart = projectionPage.As<TitlePart>();
            titlePart.Title = "Events";

            var autoRoutePart = projectionPage.As<AutoroutePart>();
            autoRoutePart.UseCustomPattern = true;
            autoRoutePart.CustomPattern = "{Content.Slug}";
            autoRoutePart.DisplayAlias = autorouteService.Value.GenerateAlias(autoRoutePart);
            autorouteService.Value.PublishAlias(autoRoutePart);

            // Attach to main menu
            const string mainMenu = "Main Menu";
            var menu = menuService.GetMenu(mainMenu);

            // Create frontend menu
            var menuPart = projectionPage.As<MenuPart>();
            menuPart.MenuPosition = navigationManager.GetNextPosition(menu);
            menuPart.MenuText = "Events";
            menuPart.Menu = menu;

            // Create admin menu
            var adminMenuPart = projectionPage.As<AdminMenuPart>();
            adminMenuPart.AdminMenuPosition = GetDefaultPosition(adminMenuPart);
            adminMenuPart.OnAdminMenu = true;
            adminMenuPart.AdminMenuText = "Events";

            var queryName = "Event";
            var query = contentManager.Create("Query");
            query.As<TitlePart>().Title = $"{queryName} query";

            var filterRecord = new FilterRecord() {
                Category = "Content",
                Type = "ContentTypes",
                Description = queryName,
                Position = 1,
                State =
                    $"<Form><Description>{queryName}</Description>" +
                    $"<ContentTypes>{queryName}</ContentTypes></Form>"
            };

            var filterGroupRecord = new FilterGroupRecord();
            filterGroupRecord.Filters.Insert(0, filterRecord);

            var queryPart = query.As<QueryPart>();
            queryPart.FilterGroups.Clear();
            queryPart.FilterGroups.Insert(0, filterGroupRecord);

            var projectionPart = projectionPage.As<ProjectionPart>();
            projectionPart.Record.QueryPartRecord = new QueryPartRecord() {
                ContentItemRecord = queryPart.ContentItem.Record,
                FilterGroups = queryPart.FilterGroups,
                Id = queryPart.Id,
                Layouts = queryPart.Layouts,
                SortCriteria = queryPart.SortCriteria
            };
        }

        private string GetDefaultPosition(ContentPart part) {
            var settings = part.Settings.GetModel<AdminMenuPartTypeSettings>();
            var defaultPosition = settings == null ? "" : settings.DefaultPosition;
            var adminMenu = navigationManager.BuildMenu("admin");

            if (!string.IsNullOrEmpty(defaultPosition)) {
                return int.TryParse(defaultPosition, out int major)
                    ? Position.GetNextMinor(major, adminMenu)
                    : defaultPosition;
            }

            return Position.GetNext(adminMenu);
        }
    }
}
