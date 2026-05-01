namespace PITS.MVP.Core.Entities;

public enum ActivityType
{
    Work,
    Commute,
    Personal,
    Health,
    Travel,
    Study,
    Entertainment,
    Other
}

public enum VisibilityLevel
{
    Public = 1,
    Work = 2,
    Private = 3,
    Classified = 4
}

public enum DataSource
{
    Manual,
    GpsAuto,
    AiParse,
    CalendarSync,
    EmailParse,
    Import
}

public enum PlaceCategory
{
    Office,
    Home,
    Cafe,
    Station,
    ClientSite,
    Hotel,
    Restaurant,
    Gym,
    Other
}
