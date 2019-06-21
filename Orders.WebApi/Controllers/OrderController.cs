using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MessageQ;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Orders.WebApi.Publish.Shipping;

namespace Orders.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        public IMqPublisher _eventBus;
        public OrderController(IMqPublisher busPublisher)
        {
           _eventBus = busPublisher;
        }

        [HttpGet]
        public async Task<ActionResult>  Get()
        {
                await _eventBus.PublishAsync(new ShippingReqEvent { OrderId = Guid.NewGuid() });
             return Content("Order Queued");
        }
       
    }
}