using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExtraSpace.API.Models.TelegramModels
{
    public class UpdateModel
    {
        public long update_id { get; set; }
        public MessageModel message { get; set; }
    }
}
