using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MessageQ
{
    public interface IEventHandler<TEvent> where TEvent: Event
    {
        Task Handle(TEvent @event);
    }
}
