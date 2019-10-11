using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace AsyncPatternsDemos
{
    class Program
    {

        #region   #  RequestState  #

        private class RequestState
        {
            public HttpWebRequest HttpWebRequest { get; set; }
            public string WebPage { get; set; }
        }

        #endregion

        private static readonly string s_url = "http://deelay.me/2000/deelay.me/";


        static void Main(string[] args)
        {
            //Synchronous();
            //AsynchronousProgrammingModel();
            //TaskFromAPM();
            TaskExplicit();
            //TaskAsync().GetAwaiter().GetResult();

            Console.ReadKey();
        }


        #region   #  Synchronous  #


        private static void Synchronous()
        {
            Stopwatch stopWatch = Stopwatch.StartNew();

            HttpWebRequest httpWebRequest = HttpWebRequest.CreateHttp(s_url);

            HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse() ;   // Initial roundtrip to server.

            string webPage = String.Empty;

            Stream responsStream = httpWebResponse.GetResponseStream();        // Downloading the page contents.
            using (var streamReader = new StreamReader(responsStream))
            {
                webPage = streamReader.ReadToEnd();
            }

            double totalSeconds = stopWatch.Elapsed.TotalSeconds;

            Console.WriteLine(webPage);

            Console.WriteLine($"Download Time: {totalSeconds:F3} secs.");
        }


        #endregion


        #region   #  Asynchronous Programming Model (APM)  #


        private static void AsynchronousProgrammingModel()
        {
            Stopwatch stopWatch = Stopwatch.StartNew();

            HttpWebRequest httpWebRequest = HttpWebRequest.CreateHttp(s_url);

            RequestState requestState = new RequestState() { HttpWebRequest = httpWebRequest };

            IAsyncResult asyncResult = httpWebRequest.BeginGetResponse(new AsyncCallback(HttpResponseAvailable), requestState); 

            //asyncResult.AsyncWaitHandle.WaitOne();

            double totalSeconds = stopWatch.Elapsed.TotalSeconds;

            Console.WriteLine(requestState.WebPage);

            Console.WriteLine($"Download Time: {totalSeconds:F3} secs.");
        }


        private static void HttpResponseAvailable(IAsyncResult asyncResult)
        {
            string webPage = String.Empty;

            RequestState requestState = (RequestState)asyncResult.AsyncState;
            HttpWebRequest httpWebRequest = requestState.HttpWebRequest;

            HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.EndGetResponse(asyncResult);

            Stream responsStream = httpWebResponse.GetResponseStream();        // Downloading the page contents.
            using (var streamReader = new StreamReader(responsStream))
            {
                webPage = streamReader.ReadToEnd();
            }

            requestState.WebPage = webPage;
        }


        #endregion


        #region   #  Task from APM  #


        private static void TaskFromAPM()
        {
            Stopwatch stopWatch = Stopwatch.StartNew();


            HttpWebRequest httpWebRequest = HttpWebRequest.CreateHttp(s_url);

            Task<WebResponse> taskFromAPM = Task.Factory.FromAsync(httpWebRequest.BeginGetResponse, httpWebRequest.EndGetResponse, null);

            Task<string> continuationTask = taskFromAPM.ContinueWith( (t) => HttpResponseAvailableContinuation( t ) );

            string webPage = continuationTask.Result;


            double totalSeconds = stopWatch.Elapsed.TotalSeconds;
            Console.WriteLine(webPage);
            Console.WriteLine($"Download Time: {totalSeconds:F3} secs.");
        }


        private static string HttpResponseAvailableContinuation( Task<WebResponse> webResponseTask )
        {
            string webPage = String.Empty;

            HttpWebResponse httpWebResponse = (HttpWebResponse)webResponseTask.Result;

            Stream responsStream = httpWebResponse.GetResponseStream();        // Downloading the page contents.
            using( StreamReader streamReader = new StreamReader( responsStream ) )
            {
                webPage = streamReader.ReadToEnd();
            }

            return webPage;
        }


        #endregion


        #region   #  Task  #


        private static void TaskExplicit()
        {
            Stopwatch stopWatch = Stopwatch.StartNew();

            string webPage = String.Empty;

            HttpWebRequest httpWebRequest = HttpWebRequest.CreateHttp(s_url);

            Task<WebResponse> task = httpWebRequest.GetResponseAsync();

            Task<Task> finalTask = task.ContinueWith( (t) => 
            {
                HttpWebResponse httpWebResponse = (HttpWebResponse)t.Result;

                Stream responsStream = httpWebResponse.GetResponseStream();        // Downloading the page contents.
                StreamReader streamReader = new StreamReader(responsStream);

                Task<string> continuationTask2 = streamReader.ReadToEndAsync();

                return continuationTask2.ContinueWith((t2) =>
                {
                    webPage = t2.Result;
                    streamReader.Dispose();
                });

            }, TaskContinuationOptions.OnlyOnRanToCompletion);

            finalTask.Result.Wait();

            double totalSeconds = stopWatch.Elapsed.TotalSeconds;
            Console.WriteLine(webPage);
            Console.WriteLine($"Download Time: {totalSeconds:F3} secs.");
        }


        #endregion


        #region   #  Async  #


        private static async Task TaskAsync()
        {
            Stopwatch stopWatch = Stopwatch.StartNew();


            string webPage = String.Empty;

            HttpWebRequest httpWebRequest = HttpWebRequest.CreateHttp(s_url);

            HttpWebResponse httpWebResponse = (HttpWebResponse)(await httpWebRequest.GetResponseAsync());

            Stream responsStream = httpWebResponse.GetResponseStream();        // Downloading the page contents.
            using (StreamReader streamReader = new StreamReader(responsStream))
            {
                webPage = await streamReader.ReadToEndAsync();
            }
          

            double totalSeconds = stopWatch.Elapsed.TotalSeconds;
            Console.WriteLine(webPage);
            Console.WriteLine($"Download Time: {totalSeconds:F3} secs.");
        }


        #endregion


    }
}
