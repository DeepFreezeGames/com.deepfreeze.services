namespace Services.Runtime
{
    public enum ServiceState
    {
        Stopped = 0,
        Starting = 1,
        Error = 2,
        Running = 3,
        Stopping = 4
    }
}