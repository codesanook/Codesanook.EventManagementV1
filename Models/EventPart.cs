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

        public DateTime? StartDateTimeUtc {
            get => Record.StartDateTimeUtc;
            set => Record.StartDateTimeUtc = value;
        }

        public DateTime? FinishDateTimeUtc {
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

        public string GetFormattedSchedule() {
            const string dateFormat = "d MMM yyyy";
            // 1 May 2020, same day
            if (StartDateTimeUtc.Value.Date == FinishDateTimeUtc.Value.Date) {
                return StartDateTimeUtc.Value.ToString(dateFormat);
            }

            // 1 - 10 May 2020
            if ((StartDateTimeUtc.Value.Month == FinishDateTimeUtc.Value.Month)
                && (StartDateTimeUtc.Value.Year == FinishDateTimeUtc.Value.Year)) {
                return StartDateTimeUtc.Value.Day.ToString() + " - " + FinishDateTimeUtc.Value.ToString(dateFormat);
            }

            // 1 May - 10 June 2020
            if (StartDateTimeUtc.Value.Year == FinishDateTimeUtc.Value.Year) {
                return StartDateTimeUtc.Value.ToString("d MMM") + " - " + FinishDateTimeUtc.Value.ToString(dateFormat);
            }

            // 1 May 2020 - 1 May 2021
            return StartDateTimeUtc.Value.ToString(dateFormat) + " - " + FinishDateTimeUtc.Value.ToString(dateFormat);
        }

    }
}

