using ExtraSpace.API.Models;
using ExtraSpace.API.Models.OrdersModels;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExtraSpace.API.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;
        public OrderRepository(IConfiguration configuration)
        {
            this._configuration = configuration;
            this._connectionString = _configuration.GetConnectionString("MainConnectionString");
        }
        public ApiResponse<List<OrderModel>> GetList() =>
            ApiResponse<List<OrderModel>>.DoMethod(resp =>
            {
                using (AutoDataManager.AutoDataManager manager = new AutoDataManager.AutoDataManager(_connectionString))
                    resp.Data = manager.GetList<OrderModel>("SELECT * FROM dbo.Orders (NOLOCK) WHERE IsDeleted = 0");
            });

        public ApiResponse<byte[]> GetReport() =>
            ApiResponse<byte[]>.DoMethod(resp =>
                resp.Data = AutoDataManager.AutoDataManager.WriteToExcel(GetList().GetResultIfNotError()));

        public ApiResponse<OrderModel> AddOrder(OrderModel order) =>
            ApiResponse<OrderModel>.DoMethod(resp =>
            {
                if (order == null)
                    resp.Throw(-2, "Empty_Data");

                if (string.IsNullOrEmpty(order.Phone))
                    resp.Throw(1, "Не пережан номер телефона");

                if (string.IsNullOrEmpty(order.ClientName))
                    resp.Throw(2, "Не передано имя клиента");

                if (order.ClientName.Length > 60)
                    resp.Throw(3, "Имя слишком длинное");

                if (!string.IsNullOrEmpty(order.Comment) && order.Comment.Length > 300)
                    resp.Throw(4, "Комментарий не может быть длиннее 300 символов");

                if (order.Phone.Length != 11)
                    resp.Throw(5, "Неверный формат номера телефона");

                using (AutoDataManager.AutoDataManager manager = new AutoDataManager.AutoDataManager(_connectionString))
                    order.Id = manager.InsertModel(order, true);

                resp.Data = order;
            });
    }

    public interface IOrderRepository
    {
        ApiResponse<List<OrderModel>> GetList();
        ApiResponse<OrderModel> AddOrder(OrderModel order);
        ApiResponse<byte[]> GetReport();
    }
}
