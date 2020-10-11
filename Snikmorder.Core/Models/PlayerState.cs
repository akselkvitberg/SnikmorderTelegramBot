namespace Snikmorder.Core.Models
{
    public enum PlayerState
    {
        Started = 0,
        GivingName = 1,
        GivingAgentName = 2,
        GivingSelfie = 3,
        ConfirmApplication = 4,

        WaitingForAdminApproval = 5,
        PickedForAdminApproval = 6,

        WaitingForGameStart = 7,

        Active = 8,

        ConfirmKill = 9,

        ReportingKilling = 10,

        Killed = 11,

        Winner = 12,

    }
}