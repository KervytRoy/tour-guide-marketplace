namespace TourGuideMarketplace.Domain.Trust;

public enum PhoneContactStatus
{
    Pending = 0,
    Responded = 1,
    NoResponse = 2,
    NumberDoesNotExist = 3,
    WrongNumber = 4,
    Busy = 5,
    DifferentPerson = 6,
    RescheduleRequested = 7
}
