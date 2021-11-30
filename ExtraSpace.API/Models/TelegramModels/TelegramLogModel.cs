using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExtraSpace.API.Models.TelegramModels
{
    [AutoDataManager.Table("dbo.TelegramLogs")]
    public class TelegramLogModel
    {
        [AutoDataManager.PrimaryKey]
        [AutoDataManager.NotInsert]
        [AutoDataManager.NotUpdate]
        public int Id { get; set; }
        public long ClientId { get; set; }
        public string JsonLog { get; set; }
        public DateTime InsertDate { get; set; }
    }
}
