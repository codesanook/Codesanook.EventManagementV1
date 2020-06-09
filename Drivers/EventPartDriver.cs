using System;
using Codesanook.EventManagement.Models;
using Codesanook.EventManagement.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Common.Models;
using Orchard.Core.Common.ViewModels;
using Orchard.Localization;
using Orchard.Localization.Services;
using Orchard.UI.Notify;
// For cref
using Orchard.DisplayManagement.Implementation;
using Orchard.Logging;
using System.Collections.Generic;
using Orchard.Services;
using System.Linq;
using Orchard.Core.Common.Settings;
using System.Web;

namespace Codesanook.EventManagement.Drivers {
    //To have a part show in content type item, we need to have content part driver 
    public class EventPartDriver : ContentPartDriver<EventPart> {
        private readonly IOrchardServices orchardServices;
        private readonly IDateLocalizationServices dateLocalizationServices;
        private readonly IEnumerable<IHtmlFilter> htmlFilters;
        protected override string Prefix => nameof(EventPart);

        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        public EventPartDriver(
            IOrchardServices orchardServices,
            IDateLocalizationServices dateLocalizationServices,
            IEnumerable<IHtmlFilter> htmlFilters
        ) {
            this.orchardServices = orchardServices;
            this.dateLocalizationServices = dateLocalizationServices;
            this.htmlFilters = htmlFilters;

            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        protected override DriverResult Display(
            EventPart part,
            string displayType,
            dynamic shapeHelper
        ) {
            //This will render View/Parts/Event.cshtml with model which has 
            // property as arguments of Parts_Event () method
            var commonPart = part.As<CommonPart>();
            return Combined(
                ContentShape(
                    "Parts_Event_Meta",
                    () => {
                        // EventDisplayViewModel must inherit from Shape
                        EventDisplayViewModel viewModel = shapeHelper.Parts_Event_Meta(typeof(EventDisplayViewModel));
                        // This can be used object mapper.
                        SetValuesViewModel(part, viewModel);
                        return viewModel;
                    }
                ),
                ContentShape(
                    "Parts_Event_Body_Summary",
                    () => {
                        var favor = GetFlavor(part.As<BodyPart>());
                        var bodyText = htmlFilters.Aggregate(
                            part.Details,
                            (text, filter) => filter.ProcessContent(text, favor)
                        );

                        // EventDisplayViewModel must inherit from Shape
                        return shapeHelper.Parts_Event_Body_Summary(Html: new HtmlString(bodyText));
                    }
                ),
                ContentShape(
                    "Parts_Event",
                    () => {
                        EventDisplayViewModel viewModel = shapeHelper.Parts_Event(typeof(EventDisplayViewModel));
                        SetValuesViewModel(part, viewModel);
                        return viewModel;
                    }
                )
            );
        }

        private static string GetFlavor(BodyPart part) {
            var typePartSettings = part.Settings.GetModel<BodyTypePartSettings>();
            return (typePartSettings != null && !string.IsNullOrWhiteSpace(typePartSettings.Flavor))
                ? typePartSettings.Flavor
                : part.PartDefinition.Settings.GetModel<BodyPartSettings>().FlavorDefault;
        }

        private static void SetValuesViewModel(EventPart part, EventDisplayViewModel viewModel) {
            // We always get datetime value, we use null because of Orchard CMS
            viewModel.BeginDateTimeUtc = part.BeginDateTimeUtc.Value;
            viewModel.EndDateTimeUtc = part.EndDateTimeUtc.Value;

            viewModel.Location = part.Location;
            viewModel.MaxAttendees = part.MaxAttendees;
            viewModel.TicketPrice = part.TicketPrice;
        }

        // HTTP GET
        protected override DriverResult Editor(EventPart part, dynamic shapeHelper) {
            var viewModel = CreateViewModelFromPart(part);
            return CreateEditorShape(viewModel, shapeHelper);
        }

        // HTTP POST
        protected override DriverResult Editor(EventPart part, IUpdateModel updater, dynamic shapeHelper) {
            // Fill form data to view model
            var viewModel = new EventViewModel();
            updater.TryUpdateModel(viewModel, Prefix, null, null);

            // Fill part with Form data 
            if (!updater.TryUpdateModel(part, Prefix, null, null)) {
                orchardServices.Notifier.Error(T("Error updating Event part model from form data."));
                return CreateEditorShape(viewModel, shapeHelper);
            }

            try {
                var beginDateTimeUtc = dateLocalizationServices.ConvertFromLocalizedString(
                    viewModel.BeginDateEditor.Date,
                    viewModel.BeginTimeEditor.Time
                );
                part.BeginDateTimeUtc = beginDateTimeUtc;

                var endDateTimeUtc = dateLocalizationServices.ConvertFromLocalizedString(
                    viewModel.EndDateEditor.Date,
                    viewModel.EndTimeEditor.Time
                );
                part.EndDateTimeUtc = endDateTimeUtc;

                orchardServices.Notifier.Success(T("Event part saved"));
                return Editor(part, shapeHelper);
            }
            catch (FormatException ex) {
                const string errorMessage = "Error updating event datetime";
                orchardServices.Notifier.Error(T(errorMessage));
                Logger.Error(ex, errorMessage);

                updater.AddModelError(
                    Prefix,
                    T($"'{0}', '{1}', '{2}', '{3}' could not be parsed as a valid date and time.",
                        viewModel.BeginDateEditor.Date,
                        viewModel.EndDateEditor.Date,
                        viewModel.BeginTimeEditor.Date,
                        viewModel.EndTimeEditor.Date
                    )
                );
                return Editor(part, shapeHelper);
            }
        }

        private ContentShapeResult CreateEditorShape(EventViewModel model, dynamic shapeHelper) {
            /// More details about create shape magic and first agument can be type
            /// <see cref="DefaultShapeFactory.Create(string, Orchard.DisplayManagement.INamedEnumerable{object}, Func{dynamic})"/> constructor as a cref attribute.
            return ContentShape(
                "parts_event_edit",
                () => shapeHelper.EditorTemplate(
                    TemplateName: "Parts/Event", //inside EditorTemplates folder
                    Model: model,
                    Prefix: Prefix
                )
            );
        }

        private EventViewModel CreateViewModelFromPart(EventPart part) {
            return new EventViewModel() {
                BeginDateEditor = new DateTimeEditor() {
                    ShowDate = true,
                    ShowTime = false,
                    Date = dateLocalizationServices.ConvertToLocalizedDateString(part.BeginDateTimeUtc),
                    Time = dateLocalizationServices.ConvertToLocalizedTimeString(part.BeginDateTimeUtc),
                },
                EndDateEditor = new DateTimeEditor() {
                    ShowDate = true,
                    ShowTime = false,
                    Date = dateLocalizationServices.ConvertToLocalizedDateString(part.EndDateTimeUtc),
                    Time = dateLocalizationServices.ConvertToLocalizedTimeString(part.EndDateTimeUtc),
                },

                BeginTimeEditor = new DateTimeEditor() {
                    ShowDate = false,
                    ShowTime = true,
                    Date = dateLocalizationServices.ConvertToLocalizedDateString(part.BeginDateTimeUtc),
                    Time = dateLocalizationServices.ConvertToLocalizedTimeString(part.BeginDateTimeUtc),
                },
                EndTimeEditor = new DateTimeEditor() {
                    ShowDate = false,
                    ShowTime = true,
                    Date = dateLocalizationServices.ConvertToLocalizedDateString(part.EndDateTimeUtc),
                    Time = dateLocalizationServices.ConvertToLocalizedTimeString(part.EndDateTimeUtc),
                },

                Location = part.Location,
                MaxAttendees = part.MaxAttendees,
                TicketPrice = part.TicketPrice
            };
        }
    }
}

