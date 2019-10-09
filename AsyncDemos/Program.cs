using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace AsyncDemos
{
    class Program
    {
        static void Main(string[] args)
        {
            //ThreadOverhead();
            SharedVsLocalState();
        }


        #region   #  Thread Overhead  #


        private static void ThreadOverhead()
        {
            const int ONE_MB = 1024 * 1024;

            using (ManualResetEvent manualResetEvent = new ManualResetEvent(false))
            {
                int numberOfThreads = 0;

                try
                {
                    while (true)
                    {
                        Thread thread = new Thread(WaitOnEvent);

                        thread.Start(manualResetEvent);

                        long virtualMemorySize = Process.GetCurrentProcess().VirtualMemorySize64 / ONE_MB;
                        Console.WriteLine($"{++numberOfThreads}: {virtualMemorySize} MB");
                    }
                }
                catch (OutOfMemoryException ex)
                {
                    Console.WriteLine($"Out of memory after {numberOfThreads} threads.");
                    Debugger.Break();
                    manualResetEvent.Set();
                }
            }

        }

        private static void WaitOnEvent(object arg)
        {
            ManualResetEvent manualResetEvent = (ManualResetEvent)arg;
            manualResetEvent.WaitOne();
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
