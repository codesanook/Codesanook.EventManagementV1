using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Codesanook.EventManagement.Models {
    /// <summary>
    ///  Non content part model, suffix with Record to get auto mapping
    /// </summary>
    public class EventAttendeeRecord {

        public virtual int Id { get; set; }

        [DisplayName("First name")]
        [Required]
        public virtual string FirstName { get; set; }

        [DisplayName("Last name")]
        [Required]
        public virtual string LastName { get; set; }

        [Required]
        public virtual string Email { get; set; }

        [DisplayName("Mobile phone number")]
        [Required]
        public virtual string MobilePhoneNumber { get; set; }

        [DisplayName("Organization name")]
        [Required]
        public virtual string OrganizationName { get; set; }
    }
}