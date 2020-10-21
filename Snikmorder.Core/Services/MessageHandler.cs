using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;

namespace Snikmorder.Core.Services
{
    public class MessageHandler
    {
        private readonly ILogger<MessageHandler> _logger;
        private readonly AdminStateMachine _adminStateMachine;
        private readonly PlayerStateMachine _playerStateMachine;

        public MessageHandler(ILogger<MessageHandler> logger, AdminStateMachine adminStateMachine, PlayerStateMachine playerStateMachine)
        {
            _logger = logger;
            _adminStateMachine = adminStateMachine;
            _playerStateMachine = playerStateMachine;
        }

        public async Task<string> OnMessage(Message message)
        {
            try
            {
                if (await _adminStateMachine.IsFromAdmin(message))
                {
                    await _adminStateMachine.HandleAdminMessage(message);
                }
                else
                {
                    await _playerStateMachine.HandlePlayerMessage(message);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return e.Message + "\n" + e.StackTrace;
            }

            return "true";
        }
    }
}