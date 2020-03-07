using Codesanook.EventManagement.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Common.Models;

namespace Codesanook.EventManagement.Drivers {

    //To have a part show in content type item, we need to have content part driver 
    public class CommentPartDriver : ContentPartDriver<EventPart> {
        protected override string Prefix => nameof(EventPart);

        protected override DriverResult Display(EventPart part, string displayType, dynamic shapeHelper) {
            //This will render View/Parts/Event.cshtml with model which has 
            // property as arguments of Parts_Event () method
            var commonPart = part.As<CommonPart>();
            //commonPart.Container
            return ContentShape(
                "Parts_Event",
                () => shapeHelper.Parts_Event(
                    BeginDateTimeUtc: part.BeginDateTimeUtc
                )
            );

        }

        // HTTP GET
        protected override DriverResult Editor(EventPart part, dynamic shapeHelper) =>
            ContentShape(
                "parts_event_edit",
                () => shapeHelper.EditorTemplate(
                    TemplateName: "Parts/Event",
                    Model: part,
                    Prefix: Prefix
                )
            );

        // HTTP POST
        protected override DriverResult Editor(EventPart part, IUpdateModel updater, dynamic shapeHelper) {
            updater.TryUpdateModel(part, Prefix, null, null);
            return Editor(part, shapeHelper);
        }
    }
}
