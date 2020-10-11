using System.Threading.Tasks;
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

        public async Task OnMessage(Message message)
        {
            if (_adminStateMachine.IsFromAdmin(message))
            {
                await _adminStateMachine.HandleAdminMessage(message);
            }
            else
            {
                await _playerStateMachine.HandlePlayerMessage(message);
            }
        }
    }
}