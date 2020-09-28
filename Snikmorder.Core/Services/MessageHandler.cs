using Telegram.Bot.Types;

namespace Snikmorder.Core.Services
{
    public class MessageHandler
    {
        private readonly AdminService _adminService;
        private readonly PlayerStateMachine _playerStateMachine;

        public MessageHandler(AdminService adminService, PlayerStateMachine playerStateMachine)
        {
            _adminService = adminService;
            _playerStateMachine = playerStateMachine;
        }

        public void OnMessage(Message message)
        {
            if (_adminService.IsFromAdmin(message))
            {
                _adminService.HandleAdminMessage(message);
            }
            else
            {
                _playerStateMachine.HandlePlayerMessage(message);
            }
        }
    }

    public class AdminService
    {
        public bool IsFromAdmin(Message message)
        {
            // Todo: Detect if user is admin - stored in db?
            throw new System.NotImplementedException();
        }

        public void HandleAdminMessage(Message message)
        {
            // Handle messages such as "approve application"
            throw new System.NotImplementedException();
        }
    }
}