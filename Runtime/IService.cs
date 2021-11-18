using System;
using System.Collections.Generic;

namespace Services.Runtime
{
    public interface IService
    {
        event EventHandler Terminated;
        
        ServiceState State { get; }

        void Shutdown();
    }
}
