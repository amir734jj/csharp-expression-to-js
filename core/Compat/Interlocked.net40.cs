using System.Threading;

namespace core.Compat
{
    class Interlocked
    {
        public static void MemoryBarrier()
        {
            Thread.MemoryBarrier();
        }
    }
}
