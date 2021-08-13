using NUnit.Framework;
using HandcraftedGames.Utils.DispatchQueue;
using System.Threading.Tasks;

namespace Tests
{
    public class DispatchQueueTest: IDispatchQueueTest
    {
        public DispatchQueueTest()
        {
            var q = new DispatchQueue();
            q.PrintExceptions = false;
            dispatchQueue = q;
        }
    }
}