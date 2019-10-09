using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ThreadPoolDemo
{
    class Program
    {
        #region   #  MRE #

        private static ManualResetEvent s_mre = new ManualResetEvent(false);

        #endregion

        static void Main(string[] args)
        {
            Console.WriteLine($"Current Thread: {Thread.CurrentThread.ManagedThreadId}");

            ThreadPool.QueueUserWorkItem( new WaitCallback(DoWork) );

            //Console.WriteLine($"Is Background thread?  {Thread.CurrentThread.IsBackground}");

            #region   #  MRE #
            //s_mre.WaitOne();
            #endregion

            Console.WriteLine($"Current Thread: {Thread.CurrentThread.ManagedThreadId} Finished");

            Console.ReadLine();
        }


        private static void DoWork(object state)
        {
            Console.WriteLine($"DoWork - Current Thread: {Thread.CurrentThread.ManagedThreadId}");

            ThreadPool.QueueUserWorkItem(new WaitCallback(ContinueWithMoreWork));

            //Console.WriteLine($"Is Background thread?  {Thread.CurrentThread.IsBackground}");
        }


        private static void ContinueWithMoreWork(object state)
        {
            Console.WriteLine($"ContinueWithMoreWork - Current Thread: {Thread.CurrentThread.ManagedThreadId}");

            //Console.WriteLine($"Is Background thread?  {Thread.CurrentThread.IsBackground}");

            #region   #  MRE #

            //s_mre.Set();

            #endregion
        }
    }
}
