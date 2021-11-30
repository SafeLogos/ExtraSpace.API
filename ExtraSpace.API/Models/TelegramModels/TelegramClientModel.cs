using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExtraSpace.API.Models.TelegramModels
{
    [AutoDataManager.Table("dbo.TelegramClients")]
    public class TelegramClientModel
    {
        [AutoDataManager.PrimaryKey]
        public long Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
        public bool IsApproved { get; set; }
        public DateTime InsertDate { get; set; }
    }
}
