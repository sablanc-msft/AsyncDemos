using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EmptyBodyAsyncOverhead
{
    class Program
    {
        static void Main(string[] args)
        {
            Stopwatch sw = new Stopwatch();
            const int ITERS = 10000000;

            EmptyBody();
            EmptyBodyAsync();

            while (true)
            {
                sw.Restart();
                for (int i = 0; i < ITERS; i++)
                {
                    EmptyBody();
                }
                TimeSpan emptyBodyTime = sw.Elapsed;

                sw.Restart();
                for (int i = 0; i < ITERS; i++)
                {
                    EmptyBodyAsync();
                }
                TimeSpan emptyBodyAsyncTime = sw.Elapsed;

                Console.WriteLine("Sync  : {0}", emptyBodyTime);
                Console.WriteLine("Async : {0}", emptyBodyAsyncTime);
                Console.WriteLine("-- {0:F1}x --", emptyBodyAsyncTime.TotalSeconds / emptyBodyTime.TotalSeconds);
            }
        }


        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void EmptyBody()
        { }


        [MethodImpl(MethodImplOptions.NoInlining)]
        private static async Task EmptyBodyAsync()
        { }

      
    }
}
