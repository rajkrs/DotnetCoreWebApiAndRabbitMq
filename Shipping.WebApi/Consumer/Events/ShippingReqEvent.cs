using MessageQ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shipping.WebApi.Consumer.Events
{
    public class ShippingReqEvent : Event 
    {
        public Guid OrderId { get; set; }

        
    }
}
