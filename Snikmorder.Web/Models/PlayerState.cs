namespace SnikmorderTelegramBot.Models
{
    public enum PlayerState
    {
        Started,
        GivingName,
        GivingAgentName,
        GivingSelfie,
        ConfirmApplication,
        
        WaitingForAdminApproval,
        
        WaitingForGameStart,
        
        Active,
        
        Killing,
        WaitingForNewTarget,
        
        ReportingKilling,
        
        Killed,
        
        
        Dead,
    }
}