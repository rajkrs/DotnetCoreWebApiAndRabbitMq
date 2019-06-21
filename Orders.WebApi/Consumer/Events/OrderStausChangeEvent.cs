using MessageQ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orders.WebApi.Consumer.Events
{
    public class OrderStausChangeEvent : Event
    {
        public Guid OrderId { get; set; }
        public string Status { get; set; }

    }

}
