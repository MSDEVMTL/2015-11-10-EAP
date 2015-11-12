using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EAP_WithClass
{
    public class Program
    {
        static void Main(string[] args)
        {
            var myParam1 = new MyParam { Id = 1, Process = "DownloadBigFile", Delay = 3000, };
            var myParam2 = new MyParam { Id = 2, Process = "LoadingBigPicture", Delay = 500, };
            var myParam3 = new MyParam { Id = 3, Process = "LoadingBigPicture", Delay = 1500, };


            var myRouter = new MyRouter();
            var myAsyncWorker = new MyAsyncWorker(myRouter);

            myAsyncWorker.ExecuteAsync(myParam1);
            myAsyncWorker.ExecuteAsync(myParam2);
            myAsyncWorker.ExecuteAsync(myParam3);
            Console.ReadLine();
        }
    }

    public class MyRouter
    {
        public void RouteOnProcess(MyParam myParam)
        {
            if ("DownloadBigFile".Equals(myParam.Process))
            {
                Console.WriteLine("{0} Downloading... {1}", DateTime.Now.ToString("mm:ss.ffff"), myParam);
            }

            if ("LoadingBigPicture".Equals(myParam.Process))
            {
                Console.WriteLine("{0} Loading... {1}", DateTime.Now.ToString("mm:ss.ffff"), myParam);
            } 
        }

        public void RouteOnEnd(MyParam myParam)
        {
            if ("DownloadBigFile".Equals(myParam.Process))
            {
                Console.WriteLine("{0} Route to ctrl DownloadBigFile {1}", DateTime.Now.ToString("mm:ss.ffff"), myParam);
            }

            if ("LoadingBigPicture".Equals(myParam.Process))
            {
                Console.WriteLine("{0} Route to ctrl LoadingBigPicture {1}", DateTime.Now.ToString("mm:ss.ffff"), myParam);
            }
        }


    }


    public class MyParam
    {
        public int Delay { get; set; }
        public int Id { get; set; }
        public string Process { get; set; }

        public override string ToString()
        {
            return string.Format("Id: {0} Process: {1}", Id, Process);
        }

    }

    public class UserState
    {
        public MyParam MyParam { get; set; }
    }

    public class Request
    {
        public MyParam MyParam { get; set; }
        public int Id { get; set; }
    }

    public class MyAsyncWorker
    {
        private readonly MyRouter myRouter;

        public MyAsyncWorker(MyRouter myRouter)
        {
            this.myRouter = myRouter;
        }

        public void ExecuteAsync(MyParam param)
        {
            MyCommandProcessor myCommandProcessor = new MyCommandProcessor();
            myCommandProcessor.CommandStarted += OnCommandStarted;
            myCommandProcessor.CommandProcessed += OnCommandProcessed;
            myCommandProcessor.CommandEnded += OnCommandEnded;

            Action<Request> worker = r => myCommandProcessor.Process(r);

            Action<IAsyncResult> actionOnComplete = (requestInOnComplete) =>
            {
               // Console.WriteLine("In actionOnComplete  T:{0} - - IsThreadPool {1}", Thread.CurrentThread.ManagedThreadId, Thread.CurrentThread.IsThreadPoolThread);

                var asyncResult = (AsyncResult)requestInOnComplete;
                var asyncOperationInOnComplete = requestInOnComplete.AsyncState as AsyncOperation;
                var userStateInOnComplete = asyncOperationInOnComplete.UserSuppliedState as UserState;
                var myParam = userStateInOnComplete.MyParam;
                //Console.WriteLine("In actionOnComplete Id:{0} T:{1}- IsThreadPool {2}", myParam.Id, Thread.CurrentThread.ManagedThreadId, Thread.CurrentThread.IsThreadPoolThread);

                var inlineWorker = asyncResult.AsyncDelegate as Action<Request>;

                //Console.WriteLine("In actionOnComplete EndInvoke...  T:{0} - IsThreadPool {1}", Thread.CurrentThread.ManagedThreadId, Thread.CurrentThread.IsThreadPoolThread);
                worker.EndInvoke(requestInOnComplete);

            //    Console.WriteLine("In actionOnComplete EndInvoke... Done! param.Id:{0} == responseComplete.Id:{1} ==> {2} T:{3}",
            //        myParam.Id,
            //        commandResponse.Id,
            //        myParam.Id == commandResponse.Id,
            //        Thread.CurrentThread.ManagedThreadId);
            };


            AsyncOperation asyncOperation = AsyncOperationManager.CreateOperation(new UserState { MyParam = param, });
            var asyncCallback = new AsyncCallback(actionOnComplete);

            worker.BeginInvoke(new Request { Id = param.Id, MyParam = param, }, asyncCallback, asyncOperation);
        }

        private void OnCommandStarted(object sender, MyEventArgs e)
        {
            Console.WriteLine("{0} Started {1}", DateTime.Now.ToString("mm:ss.ffff"), e.MyParam);
        }
  
        private void OnCommandProcessed(object sender, MyEventArgs e)
        {
            //Console.WriteLine("{0} Processed {1}", DateTime.Now.ToString("mm:ss.ffff"), e.MyParam);
            myRouter.RouteOnProcess(e.MyParam);
        }

        private void OnCommandEnded(object sender, MyEventArgs e)
        {
            //Console.WriteLine("{0} Ended {1}", DateTime.Now.ToString("mm:ss.ffff"), e.MyParam);
            myRouter.RouteOnEnd(e.MyParam);
        }
    }

    public class MyEventArgs : EventArgs
    {
        public MyParam MyParam { get; set; }

    }


    public class MyCommandProcessor
    {
        public delegate void CommandStartedEventHandler(object sender, MyEventArgs e);
        public delegate void CommandProcssedEventHandler(object sender, MyEventArgs e);
        public delegate void CommandEndedEventHandler(object sender, MyEventArgs e);


        public event CommandStartedEventHandler CommandStarted;
        public event CommandProcssedEventHandler CommandProcessed;
        public event CommandEndedEventHandler CommandEnded;

        public MyCommandProcessor()
        { }


        public void Process(Request request)
        {
            if (CommandStarted != null)
            {
                CommandStarted(this, new MyEventArgs { MyParam = request.MyParam, });
            }

            System.Threading.SpinWait.SpinUntil(() => false, TimeSpan.FromMilliseconds(request.MyParam.Delay));
            if (CommandProcessed != null)
            {
                CommandProcessed(this, new MyEventArgs { MyParam = request.MyParam, });
            }

            System.Threading.SpinWait.SpinUntil(() => false, TimeSpan.FromMilliseconds(request.MyParam.Delay));
            if (CommandProcessed != null)
            {
                CommandProcessed(this, new MyEventArgs { MyParam = request.MyParam, });
            }

            if (CommandEnded != null)
            {
                CommandEnded(this, new MyEventArgs { MyParam = request.MyParam, });
            }
        }
    }



}
