using Telegram.Bot.Types;

namespace Snikmorder.Core.Services
{
    public class MessageHandler
    {
        private readonly ApprovalStateMachine _approvalStateMachine;
        private readonly PlayerStateMachine _playerStateMachine;

        public MessageHandler(ApprovalStateMachine approvalStateMachine, PlayerStateMachine playerStateMachine)
        {
            _approvalStateMachine = approvalStateMachine;
            _playerStateMachine = playerStateMachine;
        }

        public void OnMessage(Message message)
        {
            if (_approvalStateMachine.IsFromAdmin(message))
            {
                _approvalStateMachine.HandleAdminMessage(message);
            }
            else
            {
                _playerStateMachine.HandlePlayerMessage(message);
            }
        }
    }
}