using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Snikmorder.Core.Services;

namespace Snikmorder.Web.Controllers
{
    [ApiController]
    [Route("[controller]")]

    public class StatusController : ControllerBase
    {
        private readonly ILogger<StatusController> _logger;
        private readonly GameContext _context;

        public StatusController(ILogger<StatusController> logger, GameContext context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var admins = await _context.Admins.ToListAsync();
            return Ok(admins);
        }
    }
}
