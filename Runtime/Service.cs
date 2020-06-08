using System;

namespace Services.Runtime
{
    public abstract class Service : IService
    {
        private Action _onInitialize;
        public Action onInitialize => _onInitialize;
        private Action _onCleanup;
        public Action onCleanup => _onCleanup;

        public virtual void Initialize()
        {
            _onInitialize?.Invoke();
        }

        public virtual void Cleanup()
        {
            _onCleanup?.Invoke();
        }
    }
}