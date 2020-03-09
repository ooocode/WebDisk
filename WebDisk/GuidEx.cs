using Snowflake.Core;
using System;
using System.Threading;

namespace Utility
{
    public static class GuidEx
    {
        public static long NewGuid()
        {
            var worker = new IdWorker(1, 1);
            long id = worker.NextId();
            //加上当前线程id 避免多线程下相同
            return id+ Thread.CurrentThread.ManagedThreadId;
            //return DateTimeOffset.UtcNow.Ticks + Guid.NewGuid().ToString("N");
        }
    }
}
