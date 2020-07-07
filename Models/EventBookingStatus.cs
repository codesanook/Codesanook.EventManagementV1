namespace Codesanook.EventManagement.Models {
    /*
        - A user fill form
        - Show a user review  => data already saved to database with unconfirm status
        - A user not confirm and leave website
        - A user navigate to the form of the same event
        - Show form fill with information of unfirm and message link that a user can ignore old information and start over
        - If a user click start over, change status to reject/delete/cancel and show blank form
    */
    public enum EventBookingStatus {
        Uncomfirmed, // A user hasn't click confirm.
        Comfirmed, // Waiting for the payment after click confirm.

        Verifying, // After a user upload a slip.
        InvalidPayment, // Payment is not successful.  
        Successful, // A user's already paid a successful booking == verified == success

        Cancelled, // A user does not finish booking and cancels a booking.
        Refunded // A user has successful booking but cancels to get money back.
    }
}