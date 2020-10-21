using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Snikmorder.Core.Services;
using Telegram.Bot;

namespace Snikmorder.Web.Controllers
{
    [ApiController]
    [Route("[controller]")]

    public class StatusController : ControllerBase
    {
        private readonly ILogger<StatusController> _logger;
        private readonly GameContext _context;
        private readonly ITelegramBotClient _client;
        private readonly IConfiguration _configuration;

        public StatusController(ILogger<StatusController> logger, GameContext context, IConfiguration configuration, ITelegramBotClient client)
        {
            _logger = logger;
            _context = context;
            _client = client;
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var admins = await _context.Admins.ToListAsync();
            await _client.SendTextMessageAsync(admins.First().UserId, "Hello");
            //var key = _configuration.GetSection("Telegram").GetValue<string>("BotKey");

            //await _client.SendTextMessageAsync(49374973, "Hello world");

            return Ok(admins);
        }
    }
}
