using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HandcraftedGames.Utils.DispatchQueue.Runners
{
    public abstract class BaseRunner : IRunner
    {
        protected object result;
        public object Result => result;

        protected Type type;
        public Type ResultType => type;

        protected List<Task> tasks = new List<Task>();
        public IEnumerable<Task> Tasks => tasks;
    }
}