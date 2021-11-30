using ExtraSpace.API.Models;
using ExtraSpace.API.Models.OrdersModels;
using Microsoft.AspNetCore.Http;
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
        private readonly ITelegramRepository _telegramRepository;
        private readonly IHttpContextAccessor _accessor;
        private readonly string _connectionString;
        public OrderRepository(IConfiguration configuration, ITelegramRepository telegramRepository, IHttpContextAccessor accessor)
        {
            this._configuration = configuration;
            this._connectionString = _configuration.GetConnectionString("MainConnectionString");
            this._telegramRepository = telegramRepository;
            this._accessor = accessor;
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
                    resp.Throw(1, "Не передан номер телефона");

                if (string.IsNullOrEmpty(order.ClientName))
                    resp.Throw(2, "Не передано имя клиента");

                if (order.ClientName.Length > 60)
                    resp.Throw(3, "Имя слишком длинное");

                if (!string.IsNullOrEmpty(order.Comment) && order.Comment.Length > 300)
                    resp.Throw(4, "Комментарий не может быть длиннее 300 символов");

                if (order.Phone.Length != 10)
                    resp.Throw(5, "Неверный формат номера телефона");

                order.InsertDate = DateTime.Now;
                order.IsDeleted = false;
                order.IsComplete = false;
                order.IP = _accessor.HttpContext.Connection.RemoteIpAddress.ToString();
                order.Phone = "7" + order.Phone;

                List<OrderModel> orders = GetList().GetResultIfNotError();
                if (orders
                    .Where(o => (o.Phone == order.Phone || o.IP == order.IP) &&
                    o.InsertDate.Date == DateTime.Now.Date &&
                    o.InsertDate.Hour == DateTime.Now.Hour).Count() >= 3)
                    resp.Throw(6, "Слишком много заявок, обратитесь позже");

                using (AutoDataManager.AutoDataManager manager = new AutoDataManager.AutoDataManager(_connectionString))
                    order.Id = manager.InsertModel(order, true);


                _telegramRepository.NotifyAllMembersAboutNewOrder(order);

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
