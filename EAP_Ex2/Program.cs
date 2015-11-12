using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EAP_Ex2
{
    class Program
    {
        static void Main(string[] args)
        {
            var myParam = new MyParam { Id = 1, };
            var basicCommandWithThreadId = new BasicCommandWithThreadId();

            var actions = System.Linq.Enumerable.Range(0, 20).Select(i => new { Id = i, ActionToInvoke = new Action(() => basicCommandWithThreadId.SendCommand(new MyParam { Id = i, })) });
            foreach (var a in actions)
            {
                Console.WriteLine("Starting {0}", a.Id);
                a.ActionToInvoke();
            }

            Console.ReadLine();
        }
    }





    public class RequestCommand
    {
        public int Id { get; set; }
    }

    public class ResponseCompleteCommand
    {
        public int Id { get; set; }
    }

    public class ResponseCompleteEvent : EventArgs
    {
        public int Id { get; set; }
    }

    public class UserState
    {
        public MyParam MyParam { get; set; }
    }

    public class MyParam
    {
        public int Id { get; set; }
    }

    public class BasicCommandWithThreadId
    {
        public delegate void ProcessingCommandEventHandler(BasicCommandWithThreadId sender, ResponseCompleteEvent e);

        public event ProcessingCommandEventHandler ProcessingCommand;

        public void SendCommand(MyParam param)
        {
            Console.WriteLine("Start Id:{0} T:{1}- IsThreadPool {2}", param.Id, Thread.CurrentThread.ManagedThreadId, Thread.CurrentThread.IsThreadPoolThread);


            ProcessingCommand += OnProcessingCommand;

            Func<RequestCommand, ResponseCompleteCommand> worker = (inFuncRequest) =>
            {
                Console.WriteLine("In Worker Id:{0} - T:{1} - IsThreadPool {2}", inFuncRequest.Id, Thread.CurrentThread.ManagedThreadId, Thread.CurrentThread.IsThreadPoolThread);

                Console.WriteLine("In Worker ... Calling Event Id:{0} T:{1} - IsThreadPool {2}", inFuncRequest.Id, Thread.CurrentThread.ManagedThreadId, Thread.CurrentThread.IsThreadPoolThread);

                OnProcessingCommand(this, new ResponseCompleteEvent { Id = inFuncRequest.Id, });

                return new ResponseCompleteCommand { Id = inFuncRequest.Id, };
            };


            Action<IAsyncResult> actionOnComplete = (requestInOnComplete) =>
            {
                Console.WriteLine("In actionOnComplete  T:{0} - - IsThreadPool {1}", Thread.CurrentThread.ManagedThreadId, Thread.CurrentThread.IsThreadPoolThread);

                var asyncResult = (AsyncResult)requestInOnComplete;
                var asyncOperationInOnComplete = requestInOnComplete.AsyncState as AsyncOperation;
                var userStateInOnComplete = asyncOperationInOnComplete.UserSuppliedState as UserState;
                var myParam = userStateInOnComplete.MyParam;
                Console.WriteLine("In actionOnComplete Id:{0} T:{1}- IsThreadPool {2}", myParam.Id, Thread.CurrentThread.ManagedThreadId, Thread.CurrentThread.IsThreadPoolThread);

                var inlineWorker = asyncResult.AsyncDelegate as Func<RequestCommand, ResponseCompleteCommand>;

                Console.WriteLine("In actionOnComplete EndInvoke...  T:{0} - IsThreadPool {1}", Thread.CurrentThread.ManagedThreadId, Thread.CurrentThread.IsThreadPoolThread);
                ResponseCompleteCommand commandResponse = worker.EndInvoke(requestInOnComplete);

                Console.WriteLine("In actionOnComplete EndInvoke... Done! param.Id:{0} == responseComplete.Id:{1} ==> {2} T:{3}",
                    myParam.Id,
                    commandResponse.Id,
                    myParam.Id == commandResponse.Id,
                    Thread.CurrentThread.ManagedThreadId);
            };

            AsyncOperation asyncOperation = AsyncOperationManager.CreateOperation(new UserState { MyParam = param, });
            var asyncCallback = new AsyncCallback(actionOnComplete);

            worker.BeginInvoke(new RequestCommand { Id = param.Id, }, asyncCallback, asyncOperation);

            Console.WriteLine("End Id:{0} T:{1}- IsThreadPool {2}", param.Id, Thread.CurrentThread.ManagedThreadId, Thread.CurrentThread.IsThreadPoolThread);


            //Console.WriteLine("*************************");

            //worker(new RequestCommand { Id = 999 });

            //Console.WriteLine("*************************");
        }

        void OnProcessingCommand(BasicCommandWithThreadId sender, ResponseCompleteEvent e)
        {
            Console.WriteLine("OnProcessingCommand Id:{0} T:{1}- IsThreadPool {2}", e.Id, Thread.CurrentThread.ManagedThreadId, Thread.CurrentThread.IsThreadPoolThread);
        }
    }

}
