using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace EAP_FromScratch2
{
    class Program
    {
        static void Main(string[] args)
        {
            new AsyncRunner().RunAsync();

            Console.ReadLine();

        }
    }

    public class MyReader
    {
        public delegate void ReadingEventHandler(object sender, EventArgs eventArgs);

        public event ReadingEventHandler Reading;


        public void Read()
        {

            Reading(this, new EventArgs { });
        }
    }


    public class AsyncRunner
    {
        public void RunAsync()
        {
            var myReader = new MyReader();
            myReader.Reading += MyReader_Reading;


            Action worker = () => myReader.Read();

            Action<IAsyncResult> callback = asyncResult =>
                {
                    var inlineAsyncResult = (AsyncResult)asyncResult;
                    var inlineWorker = (Action)inlineAsyncResult.AsyncDelegate;
                    inlineWorker.EndInvoke(asyncResult);


                };

            var ascynCallBack = new AsyncCallback(callback);
            var asyncOperation = AsyncOperationManager.CreateOperation(new object { });


            worker.BeginInvoke(ascynCallBack, asyncOperation);


        }

        private void MyReader_Reading(object sender, EventArgs eventArgs)
        {
            Console.WriteLine("Reading");
        }
    }


}
