using System.Threading.Tasks;

namespace HandcraftedGames.Utils.DispatchQueue.Runners
{
    public class SimpleRunner: BaseRunner
    {
        public SimpleRunner(System.Action action)
        {
            tasks.Add(new Task(action));
        }
    }
    public class SimpleRunner<T> : BaseRunner
    {
        public SimpleRunner(System.Func<T> action)
        {
            result = typeof(T);
            tasks.Add(new Task(() => { result = action(); }));
        }
    }
}