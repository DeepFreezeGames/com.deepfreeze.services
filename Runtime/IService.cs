using System;

namespace Services.Runtime
{
    public interface IService
    {
        Action onInitialize { get; }
        Action onCleanup { get; }

        void Initialize();

        void Cleanup();
    }
}
