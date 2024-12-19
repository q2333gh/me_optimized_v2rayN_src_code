using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceLib.Handlers
{
    public sealed class PeriodicTaskHandler
    {
        private static readonly Lazy<PeriodicTaskHandler> _instance = new(() => new PeriodicTaskHandler());
        public static PeriodicTaskHandler Instance => _instance.Value;

        private readonly ConcurrentDictionary<string, (PeriodicTimer Timer, CancellationTokenSource Cts, bool Persistent)> _tasks;

        private PeriodicTaskHandler()
        {
            _tasks = new ConcurrentDictionary<string, (PeriodicTimer, CancellationTokenSource, bool)>();
        }

        public async Task AddTask(string taskId, TimeSpan interval, Func<Task> action, bool persistent = false)
        {
            // 如果任务已存在，先停止并移除
            await RemoveTask(taskId);

            var cts = new CancellationTokenSource();
            var timer = new PeriodicTimer(interval);

            // 启动任务
            _ = RunPeriodicTask(taskId, timer, action, cts.Token);

            _tasks[taskId] = (timer, cts, persistent);
        }

        private async Task RunPeriodicTask(string taskId, PeriodicTimer timer, Func<Task> action, CancellationToken ct)
        {
            try
            {
                while (await timer.WaitForNextTickAsync(ct))
                {
                    try
                    {
                        await action();
                    }
                    catch (Exception ex)
                    {
                        Logging.SaveLog($"PeriodicTask {taskId} failed: {ex.Message}", ex);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // 正常取消，不需要处理
            }
        }

        public async Task RemoveTask(string taskId)
        {
            if (_tasks.TryRemove(taskId, out var task))
            {
                task.Cts.Cancel();
                task.Timer.Dispose();
                task.Cts.Dispose();
            }
        }

        public async Task Stop()
        {
            foreach (var taskId in _tasks.Keys)
            {
                // 跳过持久化的任务
                if (_tasks.TryGetValue(taskId, out var task) && task.Persistent)
                {
                    continue;
                }
                await RemoveTask(taskId);
            }
        }
    }
} 