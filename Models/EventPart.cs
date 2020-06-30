using System;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Core.Common.Models;
using Orchard.Core.Title.Models;
using Orchard.Security;

namespace Codesanook.EventManagement.Models {
    public class EventPart : ContentPart<EventPartRecord> {
        public string Title {
            get => this.As<TitlePart>().Title;
            set => this.As<TitlePart>().Title = value;
        }

        public string Details {
            get => this.As<BodyPart>().Text;
            set => this.As<BodyPart>().Text = value;
        }

        public IUser Creator {
            get => this.As<ICommonPart>().Owner;
            set => this.As<ICommonPart>().Owner = value;
        }

        public bool IsPublished =>
            ContentItem.VersionRecord != null && ContentItem.VersionRecord.Published;

        public bool HasDraft =>
            (
                (ContentItem.VersionRecord != null) &&
                ((ContentItem.VersionRecord.Published == false) ||
                (ContentItem.VersionRecord.Published && ContentItem.VersionRecord.Latest == false))
            );

        public bool HasPublished =>
            IsPublished || ContentItem.ContentManager.Get(Id, VersionOptions.Published) != null;

        public DateTime? PublishedUtc => this.As<ICommonPart>().PublishedUtc;

        public DateTime? BeginDateTimeUtc {
            get => Record.StartDateTimeUtc;
            set => Record.StartDateTimeUtc = value;
        }

        public DateTime? EndDateTimeUtc {
            get => Record.FinishDateTimeUtc;
            set => Record.FinishDateTimeUtc = value;
        }

        public string Location {
            get => Record.Location;
            set => Record.Location = value;
        }

        public virtual int MaxAttendees {
            get => Record.MaxAttendees;
            set => Record.MaxAttendees = value;
        }

        public decimal TicketPrice {
            get => Record.TicketPrice;
            set => Record.TicketPrice = value;
        }
    }
}

