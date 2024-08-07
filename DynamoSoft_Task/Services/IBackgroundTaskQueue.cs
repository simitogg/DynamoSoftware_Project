namespace DynamoSoft_Task.Services
{
    public interface IBackgroundTaskQueue
    {
        void QueueBackgroundWorkItem(Func<CancellationToken, Task> workItem, string sessionId);

        Task ExecuteAsync(CancellationToken stoppingToken);
        bool IsTaskRunning(string sessionId);
    }
}
