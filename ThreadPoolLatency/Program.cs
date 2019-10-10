using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ThreadPoolLatency
{
    class Program
    {
        const int ITERATIONS = 100;
        static CountdownEvent done = new CountdownEvent(ITERATIONS);

        static readonly HttpClient s_httpClient = new HttpClient() { Timeout = TimeSpan.FromSeconds( 180 ) };

        static DateTime startTime = DateTime.UtcNow;
        static TimeSpan totalLatency = TimeSpan.FromSeconds(0);
        static TimeSpan totalDuration = TimeSpan.FromSeconds(0);

        static SynchronizedCollection<string> messages = new SynchronizedCollection<string>();


        public static void Main(string[] args)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            //Min_Threads_Default();

            //Average latency = 2131.2303063 ms.
            //Average duration = 2443.2695467 ms.
            //Run Time = 25294.4921 ms.


            //Min_Threads_Default_LongRunning();

            //Average latency = 13.350484 ms.
            //Average duration = 23093.8704251 ms.
            //Run Time = 66305.284 ms.


            //Min_Threads_1000();

            //Average latency 1st = 2.0492003 ms.
            //Average latency 2nd = 0.7739195 ms.

            //Average duration 1st = 24867.8671921 ms.
            //Average duration 2nd = 18700.290246 ms.

            //Run Time 1st = 67063.3595 ms.
            //Run Time 2nd = 62983.2131 ms.


            //Async();

            //Average latency 1st = 0.0421205 ms.
            //Average latency 2nd = 0.0205592 ms.

            //Average duration 1st = 42040.5327851 ms.
            //Average duration 2nd = 42195.1617574 ms.

            //Run Time 1st = 74471.7295 ms.
            //Run Time 2nd = 74257.0167 ms.


            Async_Min_1000();

            //Average latency 1st = 0.0265977 ms.
            //Average latency 2nd = 0.0118727 ms.
            
            //Average duration 1st = 40119.0676717 ms.
            //Average duration 2nd = 42996.6848702 ms.
            
            //Run Time 1st = 72329.1381 ms.
            //Run Time 2nd = 76002.9685 ms.


            ThreadPool.GetMinThreads(out int minWorkerThreads, out int minCompletionPortThreads);
            ThreadPool.GetMaxThreads(out int maxWorkerThreads, out int maxCompletionPortThreads);

            Console.WriteLine($"\nMin: {minWorkerThreads}  Max: {maxWorkerThreads}    Min_IO: {minCompletionPortThreads}  Max_IO: {maxCompletionPortThreads}" );

            Console.ReadLine();
        }


        public static void Min_Threads_Default()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            for (int i = 0; i < ITERATIONS; i++)
            {
                DateTime queueTime = DateTime.UtcNow;
                int id = i;

                Task.Run( () =>
                {
                    OnTaskStart(id, queueTime);
                    NetDelay();
                    OnTaskEnd(id, queueTime);
                });

                Thread.Sleep(20);
            }

            done.Wait();
            stopwatch.Stop();

            Console.WriteLine($"\nAverage latency = {totalLatency.TotalMilliseconds / ITERATIONS} ms.");
            Console.WriteLine($"\nAverage duration = {totalDuration.TotalMilliseconds / ITERATIONS} ms.");
            Console.WriteLine($"\nRun Time = {stopwatch.Elapsed.TotalMilliseconds} ms.");
        }


        public static void Min_Threads_Default_LongRunning()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            for (int i = 0; i < ITERATIONS; i++)
            {
                DateTime queueTime = DateTime.UtcNow;
                int id = i;

                Task.Factory.StartNew(() =>
                {
                    OnTaskStart(id, queueTime);

                    NetDelay();

                    OnTaskEnd(id, queueTime);
                }, TaskCreationOptions.LongRunning);

                Thread.Sleep(20);
            }

            done.Wait();
            stopwatch.Stop();

            Console.WriteLine($"\nAverage latency = {totalLatency.TotalMilliseconds / ITERATIONS} ms.");
            Console.WriteLine($"\nAverage duration = {totalDuration.TotalMilliseconds / ITERATIONS} ms.");
            Console.WriteLine($"\nRun Time = {stopwatch.Elapsed.TotalMilliseconds} ms.");
        }


        public static void Min_Threads_1000()
        {
            ThreadPool.SetMinThreads(1000, 1000);

            Stopwatch stopwatch = Stopwatch.StartNew();

            for (int i = 0; i < ITERATIONS; i++)
            {
                DateTime queueTime = DateTime.UtcNow;

                int id = i;

                Task.Run(() =>
                {
                    OnTaskStart(id, queueTime);
                    NetDelay();
                    OnTaskEnd(id, queueTime);
                });

                Thread.Sleep(20);
            }

            done.Wait();
            stopwatch.Stop();

            double averageLatency = totalLatency.TotalMilliseconds / ITERATIONS;
            double averageDuration = totalDuration.TotalMilliseconds / ITERATIONS;
            double runtimeInMilliseconds = stopwatch.Elapsed.TotalMilliseconds;

            startTime = DateTime.UtcNow;
            totalLatency = TimeSpan.FromSeconds(0);
            totalDuration = TimeSpan.FromSeconds(0);
            done.Reset();
            stopwatch.Restart();

            for (int i = 0; i < ITERATIONS; i++)
            {
                DateTime queueTime = DateTime.UtcNow;

                int id = i;

                Task.Run(() =>
                {
                    OnTaskStart(id, queueTime);
                    NetDelay();
                    OnTaskEnd(id, queueTime);
                });

                Thread.Sleep(20);
            }

            done.Wait();
            stopwatch.Stop();

            Console.WriteLine($"\nAverage latency 1st = {averageLatency} ms.");
            Console.WriteLine($"\nAverage latency 2nd = {totalLatency.TotalMilliseconds / ITERATIONS} ms.");

            Console.WriteLine($"\nAverage duration 1st = {averageDuration} ms.");
            Console.WriteLine($"\nAverage duration 2nd = {totalDuration.TotalMilliseconds / ITERATIONS} ms.");

            Console.WriteLine($"\nRun Time 1st = {runtimeInMilliseconds} ms.");
            Console.WriteLine($"\nRun Time 2nd = {stopwatch.Elapsed.TotalMilliseconds} ms.");
        }


        public static void Async()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            for (int i = 0; i < ITERATIONS; i++)
            {
                DateTime queueTime = DateTime.UtcNow;

                int id = i;

                Task.Run(async () =>
                {
                    OnTaskStart(id, queueTime);
                    await NetDelayAsync();
                    OnTaskEnd(id, queueTime);
                });

                Thread.Sleep(10);
            }

            done.Wait();
            stopwatch.Stop();

            double averageLatency = totalLatency.TotalMilliseconds / ITERATIONS;
            double averageDuration = totalDuration.TotalMilliseconds / ITERATIONS;
            double runtimeInMilliseconds = stopwatch.Elapsed.TotalMilliseconds;

            startTime = DateTime.UtcNow;
            totalLatency = TimeSpan.FromSeconds(0);
            totalDuration = TimeSpan.FromSeconds(0);
            done.Reset();
            stopwatch.Restart();

            for (int i = 0; i < ITERATIONS; i++)
            {
                DateTime queueTime = DateTime.UtcNow;

                int id = i;

                Task.Run(async () =>
                {
                    OnTaskStart(id, queueTime);
                    await NetDelayAsync();
                    OnTaskEnd(id, queueTime);
                });

                Thread.Sleep(10);
            }

            done.Wait();
            stopwatch.Stop();

            Console.WriteLine($"\nAverage latency 1st = {averageLatency} ms.");
            Console.WriteLine($"\nAverage latency 2nd = {totalLatency.TotalMilliseconds / ITERATIONS} ms.");

            Console.WriteLine($"\nAverage duration 1st = {averageDuration} ms.");
            Console.WriteLine($"\nAverage duration 2nd = {totalDuration.TotalMilliseconds / ITERATIONS} ms.");

            Console.WriteLine($"\nRun Time 1st = {runtimeInMilliseconds} ms.");
            Console.WriteLine($"\nRun Time 2nd = {stopwatch.Elapsed.TotalMilliseconds} ms.");
        }


        public static void Async_Min_1000()
        {
            ThreadPool.SetMinThreads(1000, 1000);

            Async();
        }




        static void OnTaskStart(int id, DateTime queueTime)
        {
            DateTime now = DateTime.UtcNow;
            TimeSpan latency = now - queueTime;
            TimeSpan timestamp = now - startTime;

            lock (done)
            {
                totalLatency += latency;
            }

            string msg = string.Format($"{timestamp}: Started {id,3}, latency = {latency}");

            messages.Add(msg);
            Console.WriteLine(msg);
        }


        static void OnTaskEnd(int id, DateTime queueTime)
        {
            DateTime now = DateTime.UtcNow;
            TimeSpan duration = now - queueTime;
            TimeSpan timestamp = now - startTime;

            lock (done)
            {
                totalDuration += duration;
            }

            string msg = string.Format($"{timestamp}: Finished {id,3}, duration = { duration}");

            messages.Add(msg);
            Console.WriteLine(msg);

            done.Signal();
        }


        static void NetDelay()
        {
            HttpWebRequest httpWebRequest = HttpWebRequest.CreateHttp("http://deelay.me/500/deelay.me/");

            HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();   // Initial roundtrip to server.

            string webPage = String.Empty;

            Stream responsStream = httpWebResponse.GetResponseStream();        // Downloading the page contents.
            using (var streamReader = new StreamReader(responsStream))
            {
                webPage = streamReader.ReadToEnd();
            }
        }

        static Task NetDelayAsync()
        {
            return s_httpClient.GetStringAsync("http://deelay.me/500/deelay.me/");
        }
        

        //static void DoCpuWork(int milliseconds)
        //{
        //    DateTime endTime = DateTime.UtcNow.AddMilliseconds( milliseconds);

        //    while (DateTime.UtcNow < endTime)
        //    { }
        //}

    }

}
