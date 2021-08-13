using System.Collections.Generic;
using NUnit.Framework;
using HandcraftedGames.Utils.DispatchQueue;
using System.Threading.Tasks;
using System.Threading;

namespace Tests
{
    public abstract class IDispatchQueueTest
    {
        protected IDispatchQueue dispatchQueue;
        protected TaskScheduler schedulerToRunTestsOn;

        [Test]
        public void SimpleActionTypeless()
        {
            var numbers = new List<int>();
            LaunchSync(async () => {
                PrintThread("At sync test task launch");
                numbers.Add(0);
                var task = dispatchQueue.EnqueueAsync(() => {
                    Thread.Sleep(100);
                    PrintThread("Sub task");
                    numbers.Add(1);
                });
                PrintThread("before await");
                await task;
                numbers.Add(2);
                PrintThread("after await");
            });
            Assert.AreEqual(new List<int> { 0, 1, 2 }, numbers);
        }

        [Test]
        public void SimpleActionTypelessThrow()
        {
            var numbers = new List<int>();
            Assert.Throws<System.AggregateException>(() => 
            {
                LaunchSync(async () => {
                    PrintThread("At sync test task launch");
                    numbers.Add(0);
                    System.Action action = () => {
                        PrintThread("Sub task");
                        throw new System.Exception("Internal throw");
                    };
                    var task = dispatchQueue.EnqueueAsync(action);
                    PrintThread("before await");
                    await task;
                    numbers.Add(2);
                    PrintThread("after await");
                });
            });
        }

        [Test]
        public void SimpleActionWithReturnValue()
        {
            var numbers = new List<int>();
            LaunchSync(async () => {
                PrintThread("At sync test task launch");
                numbers.Add(0);
                var task = dispatchQueue.EnqueueAsync(() => {
                    Thread.Sleep(100);
                    PrintThread("Sub task");
                    return 1;
                });
                PrintThread("before await");
                numbers.Add(await task);
                numbers.Add(2);
                PrintThread("after await");
            });
            Assert.AreEqual(new List<int> { 0, 1, 2 }, numbers);
        }
        [Test]
        public void SimpleActionWithReturnValueThrow()
        {
            var numbers = new List<int>();
            Assert.Throws<System.AggregateException>(() => 
            {
                LaunchSync(async () => {
                    PrintThread("At sync test task launch");
                    numbers.Add(0);
                    System.Func<int> action = () => {
                        Thread.Sleep(100);
                        PrintThread("Sub task");
                        throw new System.Exception("Internal throw");
                    };
                    var task = dispatchQueue.EnqueueAsync(action);
                    PrintThread("before await");
                    numbers.Add(await task);
                    numbers.Add(2);
                    PrintThread("after await");
                });
                // Assert.AreEqual(new List<int> { 0, 1, 2 }, numbers);
            });
        }

        [Test]
        public void DeferredRunnerWithNestedAwaitsAndValue()
        {
            var numbers = new List<int>();
            LaunchSync(async () => {
                PrintThread("At sync test task launch");
                numbers.Add(0);
                var task = dispatchQueue.EnqueueAsync(async () => {
                    PrintThread("Sub task before await");
                    await Task.Run(async () => await Task.Delay(1));
                    PrintThread("await 0");
                    await Task.Run(async () => await Task.Run(async () => await Task.Delay(1)));
                    PrintThread("await 1");
                    await Task.Run(async () => await Task.Delay(1));
                    PrintThread("await 2");
                    Thread.Sleep(1);
                    PrintThread("Sub task");
                    numbers.Add(1);
                    return 12;
                });
                PrintThread("before await");
                await task;
                numbers.Add(2);
                PrintThread("after await");
            });
            Assert.AreEqual(new List<int> { 0, 1, 2 }, numbers);
        }

        [Test]
        public void DeferredRunnerWithNestedAwaitsAndValueThrow()
        {
            var numbers = new List<int>();
            Assert.Throws<System.AggregateException>(() => 
            {
                LaunchSync(async () => {
                    PrintThread("At sync test task launch");
                    numbers.Add(0);
                    var task = dispatchQueue.EnqueueAsync(async () => {
                        PrintThread("Sub task before await");
                        await Task.Run(async () => await Task.Delay(1));
                        PrintThread("await 0");
                        await Task.Run(async () => await Task.Run(async () => await Task.Delay(1)));
                        PrintThread("await 1");
                        await Task.Run(async () => await Task.Delay(1));
                        await Task.Run(() => throw new System.Exception("internal throw"));
                        PrintThread("await 2");
                        Thread.Sleep(1);
                        PrintThread("Sub task");
                        numbers.Add(1);
                        return 12;
                    });
                    PrintThread("before await");
                    await task;
                    numbers.Add(2);
                    PrintThread("after await");
                });
            });
        }

        [Test]
        public void DeferredRunnerTypelessWithNestedAwaits()
        {
            var numbers = new List<int>();
            LaunchSync(async () => {
                PrintThread("At sync test task launch");
                numbers.Add(0);
                var task = dispatchQueue.EnqueueAsync(async () => {
                    PrintThread("Sub task before await");
                    await Task.Run(async () => await Task.Delay(1));
                    PrintThread("await 0");
                    await Task.Run(async () => await Task.Run(async () => await Task.Delay(1)));
                    PrintThread("await 1");
                    await Task.Run(async () => await Task.Delay(1));
                    PrintThread("await 2");
                    Thread.Sleep(1);
                    PrintThread("Sub task");
                    numbers.Add(1);
                });
                PrintThread("before await");
                await task;
                numbers.Add(2);
                PrintThread("after await");
            });
            Assert.AreEqual(new List<int> { 0, 1, 2 }, numbers);
        }

        [Test]
        public void DeferredRunnerTypelessWithNestedAwaitsThrow()
        {
            var numbers = new List<int>();
            Assert.Throws<System.AggregateException>(() => 
            {
                LaunchSync(async () => {
                    PrintThread("At sync test task launch");
                    numbers.Add(0);
                    var task = dispatchQueue.EnqueueAsync(async () => {
                        PrintThread("Sub task before await");
                        await Task.Run(async () => await Task.Delay(1));
                        PrintThread("await 0");
                        await Task.Run(async () => await Task.Run(async () => await Task.Delay(1)));
                        PrintThread("await 1");
                        await Task.Run(() => throw new System.Exception("Internal throw"));
                        PrintThread("await 2");
                        Thread.Sleep(1);
                        PrintThread("Sub task");
                        numbers.Add(1);
                    });
                    PrintThread("before await");
                    await task;
                    numbers.Add(2);
                    PrintThread("after await");
                });
            });
        }
        
        public void PrintThread(string comment = null)
        {
            if(comment != null)
                UnityEngine.Debug.Log("Thread test: " + System.Threading.Thread.CurrentThread.ManagedThreadId + " [" + comment + "]");
            else
                UnityEngine.Debug.Log("Thread test: " + System.Threading.Thread.CurrentThread.ManagedThreadId);
        }

        // public void LaunchSync(System.Action action)
        // {
        //     var task = Task.Run(() => action());
        //     try { task.Wait(10000); }
        //     catch (System.Exception e) { UnityEngine.Debug.LogError("Intercepted exception: " + e); }
        // }

        public void LaunchSync(System.Func<Task> action)
        {
            var task = Task.Run(async () => await action());
            task.Wait(10000);
            // catch (System.Exception e) { UnityEngine.Debug.LogError("Intercepted exception: " + e); }
        }
    }
}