using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExtraSpace.API.Models.OrdersModels
{
    [AutoDataManager.Table("Orders")]
    public class OrderModel
    {
        [AutoDataManager.PrimaryKey]
        [AutoDataManager.NotInsert]
        [AutoDataManager.NotUpdate]
        public int Id { get; set; }

        public string Phone { get; set; }
        public string ClientName { get; set; }
        public string Comment { get; set; }
        public bool IsComplete { get; set; }
        public DateTime InsertDate { get; set; }
        public bool IsDeleted { get; set; }
    }
}
