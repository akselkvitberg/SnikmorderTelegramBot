using Telegram.Bot.Types;

namespace Snikmorder.Core.Services
{
    public class MessageHandler
    {
        private readonly AdminStateMachine _adminStateMachine;
        private readonly PlayerStateMachine _playerStateMachine;

        public MessageHandler(AdminStateMachine adminStateMachine, PlayerStateMachine playerStateMachine)
        {
            _adminStateMachine = adminStateMachine;
            _playerStateMachine = playerStateMachine;
        }

        public void OnMessage(Message message)
        {
            if (_adminStateMachine.IsFromAdmin(message))
            {
                _adminStateMachine.HandleAdminMessage(message);
            }
            else
            {
                _playerStateMachine.HandlePlayerMessage(message);
            }
        }
    }
}