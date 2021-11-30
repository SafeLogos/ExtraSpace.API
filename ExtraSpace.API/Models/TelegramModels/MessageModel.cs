using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExtraSpace.API.Models.TelegramModels
{
    public class MessageModel
    {
        public long message_id { get; set; }
        public MessageFromModel from { get; set; }
        public MessageChatModel chat { get; set; }
        public long date { get; set; }
        public string text { get; set; }
        public List<MessageEntitieModel> entities { get; set; }
    }
}
