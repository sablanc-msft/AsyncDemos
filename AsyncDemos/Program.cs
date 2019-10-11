using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncDemos
{
    class Program
    {
        private const int ONE_MB = 1024 * 1024;

        private static ManualResetEvent s_manualResetEvent = new ManualResetEvent(false);
        

        static void Main(string[] args)
        {
            ThreadOverhead();
            //ThreadCreationTime();
            //TaskCreationTime();
            //SharedVsLocalState();

            Console.ReadKey();
        }


        #region   #  Thread Overhead  #


        private static void ThreadOverhead()
        {
            int numberOfThreads = 0;

            try
            {
                while (true)
                {
                    Thread thread = new Thread(WaitOnEvent);

                    thread.Start(s_manualResetEvent);

                    long virtualMemorySize = Process.GetCurrentProcess().VirtualMemorySize64 / ONE_MB;
                    Console.WriteLine($"{++numberOfThreads}: {virtualMemorySize} MB");
                }
            }
            catch (OutOfMemoryException ex)
            {
                Console.WriteLine($"Out of memory after {numberOfThreads} threads.");
                Debugger.Break();
            }
            finally
            {
                s_manualResetEvent.Set();
            }

        }

        private static void WaitOnEvent(object arg)
        {
            ManualResetEvent manualResetEvent = (ManualResetEvent)arg;
            manualResetEvent.WaitOne();
        }


        #endregion


        #region   #  Thread Creation Time  #


        private static void ThreadCreationTime()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            try
            {
                for (int i = 0; i < 1000; i++)
                {
                    Thread thread = new Thread(WaitOnEvent);

                    thread.Start(s_manualResetEvent);
                }

                stopwatch.Stop();

                long virtualMemorySize = Process.GetCurrentProcess().VirtualMemorySize64 / ONE_MB;

                Console.WriteLine($"Process Memory Size: {virtualMemorySize} MB");
            }
            finally
            {
                s_manualResetEvent.Set();
            }

            double averageThreadCreationTime = stopwatch.Elapsed.TotalMilliseconds / 1000;

            Console.WriteLine($"\nAverage Thread Creation Time = {averageThreadCreationTime:F3} ms.");
        }


        #endregion


        #region   #  Task Creation Time  #


        private static void TaskCreationTime()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            try
            {
                for (int i = 0; i < 1000; i++)
                {
                    Task.Factory.StartNew( () => s_manualResetEvent.WaitOne(), TaskCreationOptions.LongRunning);
                }

                stopwatch.Stop();

                long virtualMemorySize = Process.GetCurrentProcess().VirtualMemorySize64 / ONE_MB;

                Console.WriteLine($"Process Memory Size: {virtualMemorySize} MB");
            }
            finally
            {
                s_manualResetEvent.Set();
            }

            double averageThreadCreationTime = stopwatch.Elapsed.TotalMilliseconds / 1000;

            Console.WriteLine($"\nAverage Thread Creation Time = {averageThreadCreationTime:F3} ms.");
        }


        #endregion


        #region   #  Shared vs. Local state  #

        private static Random s_random = new Random(7);

        private static int s_total = 0;
        private static List<int> s_numbers = new List<int>();


        private static void SharedVsLocalState()
        {
            Console.WriteLine("Starting SharedVsLocalState method");

            Thread t1 = new Thread(new ThreadStart(DoWork));
            t1.Name = "t1";
            t1.Start();


            Thread t2 = new Thread(new ThreadStart(DoWork));
            t2.Name = "t2";
            t2.Start();


            Thread t3 = new Thread(new ThreadStart(DoWork));
            t3.Name = "t3";
            t3.Start();

            Console.WriteLine("Ending SharedVsLocalState method");

            Console.ReadLine();
        }


        private static void DoWork()
        {

            #region   #  Local  #

            int x = 10;
            Console.WriteLine($"Thread '{Thread.CurrentThread.Name}' x: {x}");

            #endregion


            #region   #  Shared  #


            //Thread.Sleep(s_random.Next(1000));

            //s_total = s_total + 1;

            //Thread.Sleep(s_random.Next(1000));

            //Console.WriteLine( $"Thread '{Thread.CurrentThread.Name}' total: {s_total}");


            #endregion


            #region   #  Shared List #


            //for (int i =0; i < 100; i++)
            //{
            //    s_numbers.Add(i);
            //}


            #endregion

        }


        #endregion



    }
}
