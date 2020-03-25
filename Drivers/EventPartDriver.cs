using System;
using Codesanook.EventManagement.Models;
using Codesanook.EventManagement.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Common.Models;
using Orchard.Core.Common.ViewModels;
using Orchard.Localization;
using Orchard.Localization.Services;

namespace Codesanook.EventManagement.Drivers {

    //To have a part show in content type item, we need to have content part driver 
    public class CommentPartDriver : ContentPartDriver<EventPart> {
        private readonly IDateLocalizationServices dateLocalizationServices;
        protected override string Prefix => nameof(EventPart);

        public CommentPartDriver(IDateLocalizationServices dateLocalizationServices) {
            this.dateLocalizationServices = dateLocalizationServices;
        }

        public Localizer T { get; set; }

        protected override DriverResult Display(
            EventPart part,
            string displayType,
            dynamic shapeHelper
        ) {
            //This will render View/Parts/Event.cshtml with model which has 
            // property as arguments of Parts_Event () method
            var commonPart = part.As<CommonPart>();
            //commonPart.Container

            return Combined(
                ContentShape("Parts_Event_Meta",
                    () => shapeHelper.Parts_Event_Meta(
                        BeginDateTimeUtc: part.BeginDateTimeUtc,
                        EndDateTimeUtc: part.EndDateTimeUtc
                    )
                ),
                ContentShape(
                    "Parts_Event",
                    () => shapeHelper.Parts_Event(Event: part)
                )
            );
        }

        // HTTP GET
        protected override DriverResult Editor(EventPart part, dynamic shapeHelper) =>
            ContentShape(
                "parts_event_edit",
                () => shapeHelper.EditorTemplate(
                    TemplateName: "Parts/Event",
                    Model: BuildViewModelFromPart(part),
                    Prefix: Prefix
                )
            );

        // HTTP POST
        protected override DriverResult Editor(EventPart part, IUpdateModel updater, dynamic shapeHelper) {

            // update part
            updater.TryUpdateModel(part, Prefix, null, null);

            var model = BuildViewModelFromPart(part);

            // fill view model with Form data 
            if (updater.TryUpdateModel(model, Prefix, null, null)) {
                try {
                    var beginDateTimeUtc = dateLocalizationServices.ConvertFromLocalizedString(
                        model.BeginDateTimeEditor.Date,
                        model.BeginDateTimeEditor.Time
                    );
                    part.BeginDateTimeUtc = beginDateTimeUtc;

                    var endDateTimeUtc = dateLocalizationServices.ConvertFromLocalizedString(
                        model.EndDateTimeEditor.Date,
                        model.EndDateTimeEditor.Time
                    );
                    part.EndDateTimeUtc = endDateTimeUtc;
                }
                catch (FormatException ex) {
                    updater.AddModelError(
                        Prefix,
                        T("'{0} {1}' could not be parsed as a valid date and time.",
                            model.BeginDateTimeEditor.Date,
                            model.BeginDateTimeEditor.Time
                        )
                    );
                }
            }

            //updater.AddModelError($"{Prefix}.{nameof(EventViewModel.EndDateTimeEditor)}", T("Invalid user name"));
            return Editor(part, shapeHelper);
        }

        private EventViewModel BuildViewModelFromPart(EventPart part) {
            return new EventViewModel() {
                BeginDateTimeEditor = new DateTimeEditor() {
                    ShowDate = true,
                    ShowTime = true,
                    Date = dateLocalizationServices.ConvertToLocalizedDateString(part.BeginDateTimeUtc),
                    Time = dateLocalizationServices.ConvertToLocalizedTimeString(part.BeginDateTimeUtc),
                },
                EndDateTimeEditor = new DateTimeEditor() {
                    ShowDate = true,
                    ShowTime = true,
                    Date = dateLocalizationServices.ConvertToLocalizedDateString(part.EndDateTimeUtc),
                    Time = dateLocalizationServices.ConvertToLocalizedTimeString(part.EndDateTimeUtc),
                },
                Location = part.Location,
                MaxAttendees = part.MaxAttendees
            };
        }

    }
}
