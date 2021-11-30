using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExtraSpace.API.Models.TelegramModels
{
    public class MessageEntitieModel
    {
        public long offset { get; set; }
        public long length { get; set; }
        public string type { get; set; }
    }
}
