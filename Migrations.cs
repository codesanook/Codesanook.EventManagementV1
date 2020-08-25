using System;
using Codesanook.EventManagement.Models;
using Orchard;
using Orchard.Autoroute.Models;
using Orchard.Autoroute.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Common.Models;
using Orchard.Core.Contents.Extensions;
using Orchard.Core.Navigation.Models;
using Orchard.Core.Navigation.Settings;
using Orchard.Core.Title.Models;
using Orchard.Data.Migration;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Projections.Models;
using Orchard.Security;
using Orchard.Settings;
using Orchard.UI.Navigation;
using Orchard.Utility;

namespace Codesanook.EventManagement {
    public class Migrations : DataMigrationImpl {
        private readonly IOrchardServices orchardServices;
        private readonly IAuthenticationService authenticationService;
        private readonly IContentManager contentManager;
        private readonly INavigationManager navigationManager;
        private readonly Lazy<IAutorouteService> autorouteService;
        private readonly ISiteService siteService;
        private readonly IMembershipService membershipService;

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        public Migrations(
            IOrchardServices orchardServices,
            IAuthenticationService authenticationService,
            IContentManager contentManager,
            INavigationManager navigationManager,
            Lazy<IAutorouteService> autorouteService,
            ISiteService siteService,
            IMembershipService membershipService
        ) {
            this.orchardServices = orchardServices;
            this.authenticationService = authenticationService;
            this.contentManager = contentManager;
            this.navigationManager = navigationManager;
            this.autorouteService = autorouteService;
            this.siteService = siteService;
            this.membershipService = membershipService;
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public int Create() {
            SchemaBuilder.CreateTable(
                nameof(EventPartRecord),
                table => table
                    .ContentPartRecord()
                    .Column<string>(nameof(EventPartRecord.Location), c => c.WithLength(1024))
                    .Column<DateTime>(nameof(EventPartRecord.StartDateTimeUtc))
                    .Column<DateTime>(nameof(EventPartRecord.FinishDateTimeUtc))
                    .Column<int>(nameof(EventPartRecord.MaxAttendees))
                    .Column<decimal>(nameof(EventPartRecord.TicketPrice))
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
                    .WithPart(
                        "BodyPart",
                        config => config
                            .WithSetting("BodyTypePartSettings.Flavor", "html")
                    )
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
                        )
                    )
                    .Draftable()
            );

            CreateProjectionItemForEventPart();

            // Create a part
            ContentDefinitionManager.AlterPartDefinition(nameof(UpcomingEventsPart),
               cfg => cfg.Attachable()
           );

            // Create a new widget content type
            // We make use of the AsWidgetWithIdentity() helper.
            const string widgetTypeName = "UpcomingEventsWidget";
            ContentDefinitionManager.AlterTypeDefinition(
                widgetTypeName,
                cfg => cfg
                    .WithPart(nameof(UpcomingEventsPart))
                    .AsWidgetWithIdentity() // in Orchard.Widget assembly
            );

            CreateEventBookingRecordTable();
            CreateEventAttendeeRecordTable();
            return 1;
        }

        public int UpdateFrom1() {
            CreateBankAccountRecordTable();
            return 2;
        }

        private void CreateEventBookingRecordTable() {
            SchemaBuilder.CreateTable(
                nameof(EventBookingRecord),
                table => table
                    .Column<int>(nameof(EventBookingRecord.Id), column => column.PrimaryKey().Identity())
                    .Column<int>("EventId", column=> column.NotNull())
                    .Column<int>("UserId", column=> column.NotNull())
                    .Column<string>(nameof(EventBookingRecord.Status), column=> column.NotNull().WithLength(32))

                    .Column<DateTime>(nameof(EventBookingRecord.BookingDateTimeUtc))
                    .Column<DateTime>(nameof(EventBookingRecord.PaidDateTimeUtc))
                    .Column<string>(
                        nameof(EventBookingRecord.PaymentConfirmationAttachementFileKey),
                        column => column.WithLength(1024)
                    )
            );
        }

        private void CreateEventAttendeeRecordTable() {
            SchemaBuilder.CreateTable(
                nameof(EventAttendeeRecord),
                table => table
                    .Column<int>(nameof(EventAttendeeRecord.Id), column => column.PrimaryKey().Identity())
                    .Column<string>(nameof(EventAttendeeRecord.FirstName), column => column.NotNull())
                    .Column<string>(nameof(EventAttendeeRecord.LastName), column => column.NotNull())

                    .Column<string>(nameof(EventAttendeeRecord.Email), column => column.NotNull())
                    .Column<string>(nameof(EventAttendeeRecord.MobilePhoneNumber), column => column.NotNull())
                    .Column<string>(nameof(EventAttendeeRecord.OrganizationName), column => column.NotNull())
                    .Column<int>("EventBookingId", column => column.NotNull())
            );
        }

        private void CreateBankAccountRecordTable() {
            SchemaBuilder.CreateTable(
                nameof(BankAccountRecord),
                table => table
                    .Column<int>(nameof(BankAccountRecord.Id), column => column.PrimaryKey().Identity())
                    .Column<string>(nameof(BankAccountRecord.BankName), column => column.NotNull().WithLength(128))
                    .Column<string>(nameof(BankAccountRecord.BranchName), column => column.NotNull().WithLength(128))
                    .Column<string>(nameof(BankAccountRecord.AccountName), column => column.NotNull().WithLength(256))
                    .Column<string>(nameof(BankAccountRecord.AccountNumber), column => column.NotNull().WithLength(10))
            );
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
                    $"<Form>" +
                    $"<ContentTypes>{queryName}</ContentTypes>" +
                    $"<Description>{queryName}</Description>" +
                    $"</Form>"
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
