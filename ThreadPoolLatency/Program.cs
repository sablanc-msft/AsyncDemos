using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ThreadPoolLatency
{
    class Program
    {
        const int ITERATIONS = 1000;
        static CountdownEvent done = new CountdownEvent(ITERATIONS);

        static DateTime startTime = DateTime.UtcNow;
        static TimeSpan totalLatency = TimeSpan.FromSeconds(0);

        static SynchronizedCollection<string> messages = new SynchronizedCollection<string>();


        public static void Main(string[] args)
        {

            V1();
            //V2();
            //V3();

            done.Wait();
            foreach (string message in messages)
            {
                Console.WriteLine(message);
            }

            Console.WriteLine( $"Average latency = {TimeSpan.FromMilliseconds(totalLatency.TotalMilliseconds / ITERATIONS)}");

            ThreadPool.GetMinThreads(out int minWorkerThreads, out int minCompletionPortThreads);

            ThreadPool.GetMaxThreads(out int maxWorkerThreads, out int maxCompletionPortThreads);

            Console.WriteLine($"Min: {minWorkerThreads}  Max: {maxWorkerThreads}    Min_IO: {minCompletionPortThreads}  Max_IO: {maxCompletionPortThreads}" );


            Console.ReadLine();
        }



       


        public static void V1()
        {
            ThreadPool.SetMaxThreads(12, 12);

            for (int i = 0; i < ITERATIONS; i++)
            {
                DateTime queueTime = DateTime.UtcNow;
                int id = i;

                ThreadPool.QueueUserWorkItem((o) =>
                {
                    OnTaskStart(id, queueTime);
                    Thread.Sleep(500);
                    OnTaskEnd(id, queueTime);
                });

                Thread.Sleep(10);
            }
        }

        public static void V2()
        {
            ThreadPool.SetMinThreads(24, 1);
            ThreadPool.SetMaxThreads(24, 1);

            for (int i = 0; i < ITERATIONS; i++)
            {
                DateTime queueTime = DateTime.UtcNow;

                int id = i;

                ThreadPool.QueueUserWorkItem((o) => 
                {
                    OnTaskStart(id, queueTime);
                    Thread.Sleep(500);
                    OnTaskEnd(id, queueTime);
                });

                Thread.Sleep(10);
            }
        }


        public static void V3()
        {
            ThreadPool.SetMaxThreads(1, 1);

            for (int i = 0; i < ITERATIONS; i++)
            {
                DateTime queueTime = DateTime.UtcNow;
                int id = i;

                ThreadPool.QueueUserWorkItem(async (o) => 
                {
                    OnTaskStart(id, queueTime);

                    await Task.Delay(500);

                    OnTaskEnd(id, queueTime);
                });

                Thread.Sleep(10);
            }
        }


        static void OnTaskStart(int id, DateTime queueTime)
        {
            TimeSpan latency = DateTime.UtcNow - queueTime;
            lock (done)
            {
                totalLatency += latency;
            }
            Log(id, queueTime, "Starting");
        }


        static void OnTaskEnd(int id, DateTime queueTime)
        {
            Log(id, queueTime, "Finished");

            done.Signal();
        }


        static void Log(int id, DateTime queueTime, string action)
        {
            DateTime now = DateTime.UtcNow;
            TimeSpan timestamp = now - startTime;
            TimeSpan latency = now - queueTime;

            string msg = string.Format($"{timestamp}: {action} {id,3}, latency = {latency}");

            messages.Add(msg);
            Console.WriteLine(msg);
        }

    }

}
