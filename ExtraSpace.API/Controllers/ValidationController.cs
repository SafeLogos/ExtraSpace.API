using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ExtraSpace.API.Models;
using ExtraSpace.API.Models.OrdersModels;
using ExtraSpace.API.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ExtraSpace.API.Controllers
{
    [ApiController]
    public class ValidationController : ControllerBase
    {
        [HttpGet(".well-known/pki-validation/E2AFA0BA2A960A0859005E6B506961FD.txt")]
       public IActionResult Get()
        {
            string text =
@"C4DD687625275717D18EBB467F683839D24C7D798A940B9483165AA31DD6131C
comodoca.com
08e876a65f6bcdc";


            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(text);
            return File(bytes, "text/plain", "E2AFA0BA2A960A0859005E6B506961FD.txt");
        }
    }
}
