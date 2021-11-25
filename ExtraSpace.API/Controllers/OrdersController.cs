using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExtraSpace.API.Models;
using ExtraSpace.API.Models.OrdersModels;
using ExtraSpace.API.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ExtraSpace.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IMailingRepository _mailingRepository;
        public OrdersController(IOrderRepository orderRepository, IMailingRepository mailingRepository)
        {
            this._orderRepository = orderRepository;
            this._mailingRepository = mailingRepository;
        }

        [HttpGet("[action]")]
        public IActionResult Test() =>
            Ok("Ok");
        
        [HttpGet("asduoa234enr5pnvd")]
        public IActionResult GetReport()
        {
            ApiResponse<byte[]> report = _orderRepository.GetReport();

            if (report.Code != 0)
                return Ok(report.Message);

            return File(report.Data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "report.xlsx");
        }

        [HttpPost("[action]")]
        public IActionResult AddOrder([FromBody] OrderModel order) =>
            Ok(this._orderRepository.AddOrder(order));
    }
}
