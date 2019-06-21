using System;
using System.Collections.Generic;
using System.Text;

namespace MessageQ
{
    public class ConsumerCollection
    {
        private Dictionary<Type, Type> _Collection;

        public Dictionary<Type, Type> Collection { get => _Collection; set => _Collection = value; }

        public ConsumerCollection()
        {
            _Collection = new Dictionary<Type, Type>();
        }
        public ConsumerCollection Add(Type @eventType, Type eventHandlerType)
        {
            _Collection.Add(@eventType, eventHandlerType);
           return this;
        }
    }
}
