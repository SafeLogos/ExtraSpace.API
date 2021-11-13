using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExtraSpace.API.Models;
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
            Ok(_mailingRepository.SendMail(new Models.MailingModels.MailModel() 
            {
                 Body = "TEST PEPEGA",
                  Title = "SENDED",
                   From = "novikov.zahar.nz@gmail.com",
                    To = "sssequencebreak@gmail.com"
            }));

        public IActionResult GetReport()
        {
            ApiResponse<byte[]> report = _orderRepository.GetReport();

            if (report.Code != 0)
                return Ok(report.Message);

            return File(report.Data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "report.xlsx");
        }
    }
}
