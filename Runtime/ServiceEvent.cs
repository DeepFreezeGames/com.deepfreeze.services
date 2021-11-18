using System;
using Events.Runtime;

namespace Services.Runtime
{
    public class ServiceEvent : IEvent
    {
        public IService Service { get; protected set; }
        public Type DispatchAs { get; protected set; }

        protected ServiceEvent(IService service)
        {
            Service = service;
            DispatchAs = typeof(ServiceEvent);
        }
    }

    public class ServiceRegisteredEvent : ServiceEvent
    {
        public ServiceRegisteredEvent(IService service) : base(service)
        {
            DispatchAs = typeof(ServiceRegisteredEvent);
        }
    }

    public class ServiceInitializedEvent : ServiceEvent
    {
        public ServiceInitializedEvent(IService service) : base(service)
        {
            DispatchAs = typeof(ServiceInitializedEvent);
        }
    }

    public class ServiceStoppedEvent : ServiceEvent
    {
        public ServiceStoppedEvent(IService service) : base(service)
        {
            DispatchAs = typeof(ServiceStoppedEvent);
        }
    }
}