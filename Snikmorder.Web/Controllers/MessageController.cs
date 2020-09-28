using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Snikmorder.Core.Services;
using Telegram.Bot.Types;

namespace SnikmorderTelegramBot.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MessageController : ControllerBase
    {
        private readonly ILogger<MessageController> _logger;
        private readonly MessageHandler _messageHandler;

        public MessageController(ILogger<MessageController> logger, MessageHandler messageHandler)
        {
            _logger = logger;
            _messageHandler = messageHandler;
        }

        [HttpPost]
        public async Task<IActionResult> TelegramMessage(Message? message)
        {
            // Send to parser

            if (message == null)
            {
                return Ok();
            }

            _messageHandler.OnMessage(message);
            return Ok();
        }
    }
}
