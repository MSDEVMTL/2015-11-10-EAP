using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace EAP_FromScratch
{
    class Program
    {
        static void Main(string[] args)
        {

            new AsyncRunner().RunAsync();
            Console.ReadLine();
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

                var reader = new MyReader();
                reader.Reading += OnRead;

                Action worker = () => reader.Read();



                Action<IAsyncResult> callback = asyncResult =>
                {
                    var asyncResult2 = (AsyncResult)asyncResult;
                    var inlineWorker = asyncResult2.AsyncDelegate as Action;
                    inlineWorker.EndInvoke(asyncResult);


                };


                var asyncOperation = AsyncOperationManager.CreateOperation(new object());
                var asyncCallback = new AsyncCallback(callback);


                worker.BeginInvoke(asyncCallback, asyncOperation);
            }

            private void OnRead(object sender, EventArgs eventArgs)
            {
                Console.WriteLine("Reading");

            }
        }

    }
}
