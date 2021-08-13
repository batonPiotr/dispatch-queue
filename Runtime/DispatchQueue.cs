using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HandcraftedGames.Utils.DispatchQueue.Runners;

namespace HandcraftedGames.Utils.DispatchQueue
{
    public class DispatchQueue: IDispatchQueue
    {
        private readonly Queue<IRunner> taskQueue = new Queue<IRunner>();
        private readonly TaskScheduler scheduler;

        public bool PrintExceptions = true;

        public DispatchQueue(TaskScheduler taskScheduler = null)
        {
            scheduler = taskScheduler;
        }

        private Task currentTask;

        private void EnqueueInternal(IRunner runner)
        {
            // Validate the task
            if (runner == null) throw new ArgumentNullException(nameof(runner));
            if(runner.Tasks == null)
                throw new System.ArgumentNullException();
            if(runner.Tasks.Count() == 0)
                throw new IndexOutOfRangeException();

            lock (taskQueue)
            {
                if (currentTask == null) StartTask(runner);
                else taskQueue.Enqueue(runner);
            }
        }
        
        private void OnTaskCompletion(Task ignored)
        {
            lock (taskQueue)
            {
                currentTask = null;
                if (taskQueue.Count > 0)
                    StartTask(taskQueue.Dequeue());
            }
        }

        private void StartTask(IRunner nextItem)
        {
            var next = nextItem.Tasks.First();
            currentTask = nextItem.Tasks.Last();
            currentTask.ContinueWith(OnTaskCompletion);

            if (next.Status == TaskStatus.Created)
            {
                if(scheduler != null)
                    next.Start(scheduler);
                else
                    next.Start();
            }

            //TODO: Make exception handling more general
            if(PrintExceptions)
            foreach(var t in nextItem.Tasks)
                t.ContinueWith(task => {
                    UnityEngine.Debug.LogError("ExtendedSerialTaskQueue task failure: " + task.Exception);
                }, TaskContinuationOptions.OnlyOnFaulted);
        }
        private async Task Enqueue(IRunner runner)
        {
            EnqueueInternal(runner);
            var summedTask = Task.WhenAll(runner.Tasks);
            await summedTask;
        }

        public async Task EnqueueAsync(Action action)
        {
            await Enqueue(new SimpleRunner(action));
        }

        public async Task EnqueueAsync(Task task)
        {
            await Enqueue(new TaskedRunner(task));
        }

        public async Task EnqueueAsync(Func<Task> action)
        {
            await Enqueue(new DeferredRunner(action));
        }

        public async Task<T> EnqueueAsync<T>(Func<T> action)
        {
            var runner = new SimpleRunner<T>(action);
            await Enqueue(runner);
            return (T)runner.Result;
        }

        public async Task<T> EnqueueAsync<T>(Func<Task<T>> action)
        {
            var runner = new DeferredRunner<T>(action);
            await Enqueue(runner);
            return (T)runner.Result;
        }

        public async Task<T> EnqueueAsync<T>(Task<T> task)
        {
            var runner = new TaskedRunner<T>(task);
            await Enqueue(runner);
            return (T)runner.Result;
        }
    }
}