using System.Threading.Tasks;

namespace HandcraftedGames.Utils.DispatchQueue.Runners
{
    public class DeferredRunner : BaseRunner
    {
        public DeferredRunner(System.Func<Task> action)
        {
            var completionSource = new TaskCompletionSource<int>();
            var execute = new Task(() =>
            {
                Task subTask = null;
                try { subTask = action(); }
                catch (System.Exception e) { completionSource.SetCanceled(); throw e; }

                subTask.ContinueWith(previous => 
                {
                    completionSource.SetResult(0);
                }, TaskContinuationOptions.OnlyOnRanToCompletion);
                subTask.ContinueWith(previous =>
                {
                    completionSource.SetException(previous.Exception);
                }, TaskContinuationOptions.OnlyOnFaulted);
                subTask.ContinueWith(previous =>
                {
                    completionSource.SetCanceled();
                }, TaskContinuationOptions.OnlyOnCanceled);

                if(subTask.Status == TaskStatus.Created)
                    subTask.Start();
            });
            tasks.Add(execute);
            tasks.Add(completionSource.Task);
        }
    }
    public class DeferredRunner<T>: BaseRunner
    {
        public DeferredRunner(System.Func<Task<T>> action)
        {
            type = typeof(T);

            var completionSource = new TaskCompletionSource<T>();

            var execute = new Task(() =>
            {
                Task<T> subTask = null;
                try { subTask = action(); }
                catch (System.Exception e) { completionSource.SetCanceled(); throw e; }
                subTask.ContinueWith(previous => 
                {
                    this.result = previous.Result;
                    completionSource.SetResult(previous.Result);
                }, TaskContinuationOptions.OnlyOnRanToCompletion);
                subTask.ContinueWith(previous =>
                {
                    completionSource.SetException(previous.Exception);
                }, TaskContinuationOptions.OnlyOnFaulted);
                subTask.ContinueWith(previous =>
                {
                    completionSource.SetCanceled();
                }, TaskContinuationOptions.OnlyOnCanceled);
                if(subTask.Status == TaskStatus.Created)
                    subTask.Start();
            });

            tasks.Add(execute);
            tasks.Add(completionSource.Task);
        }
    }
}