using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EAP_Ex1
{
	class Program
	{
		static void Main(string[] args)
		{
            var myParam = new MyParam { Id = 1, };
            var basicCommand = new BasicCommand2();

            int i2 = 0;

            var actions = System.Linq.Enumerable.Range(0, 10).Select(i => new Action(() => basicCommand.SendCommand(new MyParam { Id = i, })));
            foreach (Action a in actions)
            {
                Console.WriteLine("Starting {0}", i2++);
                a.Invoke();
            }

			Console.ReadLine();
		}
	}




    public class BasicCommand
    {
        public delegate void ProcessingCommandEventHandler(object sender, EventArgs e);

        public event ProcessingCommandEventHandler ProcessingCommand;

        public void SendCommand()
        {

            // EventHandler 
            ProcessingCommand += OnProcessingCommand;

            // Worker / Async method impl 
            Func<object, object> worker = (r) => 
			{ 
				Console.WriteLine("In Worker ... Calling Event");

                // Do stuff, download big files, compress video, etc.

				OnProcessingCommand(this, new EventArgs { });

				return r;
			};


            //IAsyncResult
            //Represents the status of an asynchronous operation
            //***AsyncState
            //Gets a user - defined object that qualifies or contains information about an asynchronous operation.


            //AsyncResult
            //Encapsulates the results of an asynchronous operation on a delegate.
            //* **AsyncDelegate
            //Gets the delegate object on which the asynchronous call was invoked.


            // On action complete, this is not the Worker (the worker is on another thread)
            // You can use this to validate that the worker will execute or log that the worker is started and you'll receive events
            Action<IAsyncResult> actionOnComplete = (requestInOnComplete) => 
			{
				Console.WriteLine("In actionOnComplete");

				var asyncResult = (AsyncResult)requestInOnComplete;
				var inlineWorker = asyncResult.AsyncDelegate as Func<object, object>;

				Console.WriteLine("In actionOnComplete EndInvoke...");
				var commandResponse = worker.EndInvoke(requestInOnComplete);

				Console.WriteLine("In actionOnComplete EndInvoke... Done!");
			};




            //AsyncOperation
            //Tracks the lifetime of an asynchronous operation.
            //***UserSuppliedState
            //Gets or sets an object used to uniquely identify an asynchronous operation.

            //AsyncOperationManager
            //Provides concurrency management for classes that support asynchronous method calls
            //* **CreateOperation(Object)
            //Returns an System.ComponentModel.AsyncOperation for tracking the duration of a particular asynchronous operation.

            //AsyncCallback
            //References a method to be called when a corresponding asynchronous operation completes.

            AsyncOperation asyncOperation = AsyncOperationManager.CreateOperation(new object());
			var asyncCallback = new AsyncCallback(actionOnComplete);


            // The worker start

            // The BeginInvoke method initiates the asynchronous call. 
            // It has the same parameters as the method that you want to execute asynchronously, plus two additional optional parameters.
            // The first parameter is an AsyncCallback delegate that references a method to be called when the asynchronous call completes. 
            // The second parameter is a user - defined object that passes information into the callback method. 
            // BeginInvoke returns immediately and does not wait for the asynchronous call to complete.
            // BeginInvoke returns an IAsyncResult, which can be used to monitor the progress of the asynchronous call.


              worker.BeginInvoke(new object(), asyncCallback, asyncOperation);
        }

		void OnProcessingCommand(object sender, EventArgs e)
		{
			Console.WriteLine("OnProcessingCommand");

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

	public class BasicCommand2
	{
		public delegate void ProcessingCommandEventHandler(BasicCommand2 sender, ResponseCompleteEvent e);

		public event ProcessingCommandEventHandler ProcessingCommand;

		public void SendCommand(MyParam param)
		{
			ProcessingCommand += OnProcessingCommand;

			Func<RequestCommand, ResponseCompleteCommand> worker = (inFuncRequest) => 
			{ 
				Console.WriteLine("In Worker Id:{0}", inFuncRequest.Id);

				Console.WriteLine("In Worker ... Calling Event Id:{0}", inFuncRequest.Id);

				OnProcessingCommand(this, new ResponseCompleteEvent { Id = inFuncRequest.Id, });

				return new ResponseCompleteCommand { Id = inFuncRequest.Id, };
			};


			Action<IAsyncResult> actionOnComplete = (requestInOnComplete) => 
			{
				Console.WriteLine("In actionOnComplete");

				var asyncResult = (AsyncResult)requestInOnComplete;
				var asyncOperationInOnComplete = requestInOnComplete.AsyncState as AsyncOperation;

				var userStateInOnComplete = asyncOperationInOnComplete.UserSuppliedState as UserState;

                var myParam = userStateInOnComplete.MyParam;

				Console.WriteLine("In actionOnComplete Id:{0}", myParam.Id);

				var inlineWorker = asyncResult.AsyncDelegate as Func<RequestCommand, ResponseCompleteCommand>;

				Console.WriteLine("In actionOnComplete EndInvoke...");
				ResponseCompleteCommand commandResponse = worker.EndInvoke(requestInOnComplete);

				Console.WriteLine("In actionOnComplete EndInvoke... Done! param.Id:{0} == responseComplete.Id:{1} ==> {2}", myParam.Id, commandResponse.Id, myParam.Id == commandResponse.Id);
			};

			AsyncOperation asyncOperation = AsyncOperationManager.CreateOperation(new UserState { MyParam = param, });
			var asyncCallback = new AsyncCallback(actionOnComplete);

			worker.BeginInvoke(new RequestCommand { Id = param.Id, }, asyncCallback, asyncOperation);
		}

		void OnProcessingCommand(BasicCommand2 sender, ResponseCompleteEvent e)
		{
			Console.WriteLine("OnProcessingCommand Id:{0}", e.Id);
		}
	}

}
