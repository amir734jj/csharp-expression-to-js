using System.Threading;

namespace core.Compat
{
    public static class Interlocked
    {
        public static void MemoryBarrier()
        {
            Thread.MemoryBarrier();
        }
    }
}
