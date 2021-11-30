using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ExtraSpace.API.Models.TelegramModels;
using ExtraSpace.API.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Telegram.Bot.Types;

namespace ExtraSpace.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TelegramController : ControllerBase
    {

        private readonly ITelegramRepository _repository;

        public TelegramController(ITelegramRepository repository)
        {
            this._repository = repository;
        }

        [HttpPost]
        public IActionResult Post([FromBody] UpdateModel update)
        {
            Task.Run(() => _repository.ProcessMessage(update));
            return Ok();
        }

        

        
    }
}
