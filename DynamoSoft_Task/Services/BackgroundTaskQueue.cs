
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace DynamoSoft_Task.Services
{
    public class BackgroundTaskQueue : IBackgroundTaskQueue
    {
        private readonly ConcurrentDictionary<string, Task> _tasks = new ConcurrentDictionary<string, Task>();
        private readonly ConcurrentQueue<(Func<CancellationToken, Task> WorkItem, string SessionId)> _workItems = new ConcurrentQueue<(Func<CancellationToken, Task> WorkItem, string SessionId)>();
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);

        public void QueueBackgroundWorkItem(Func<CancellationToken, Task> workItem, string sessionId)
        {
            if (workItem == null)
            {
                throw new ArgumentNullException(nameof(workItem));
            }

            _workItems.Enqueue((workItem, sessionId));
            _semaphore.Release();
        }

        public async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await _semaphore.WaitAsync(stoppingToken);

                if (_workItems.TryDequeue(out var workItem))
                {
                    var sessionId = workItem.SessionId;

                    // Create a task and add it to the dictionary
                    var task = workItem.WorkItem(stoppingToken);
                    if (!_tasks.TryAdd(sessionId, task))
                    {
                        // If adding failed, log or handle the error
                    }

                    try
                    {
                        await task;
                    }
                    catch (Exception ex)
                    {
                        // Handle exceptions that occur during task execution
                    }
                    finally
                    {
                        // Ensure task is removed from the dictionary when it completes
                        _tasks.TryRemove(sessionId, out _);
                    }
                }
            }
        }

        public bool IsTaskRunning(string sessionId)
        {
            return _tasks.ContainsKey(sessionId) && !_tasks[sessionId].IsCompleted;
        }
    }
}
