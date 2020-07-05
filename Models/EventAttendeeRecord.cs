namespace Codesanook.EventManagement.Models {
    /// <summary>
    ///  Non content part model, suffix with Record to get auto mapping
    /// </summary>
    public class EventAttendeeRecord {
        public virtual int Id { get; set; }
        public virtual string FirstName  { get; set; }
        public virtual string LastName { get; set; } 

        public virtual string Email { get; set; }
        public virtual string MobilePhoneNumber { get; set; }
        public virtual string OrganizationName { get; set; }
    }
}