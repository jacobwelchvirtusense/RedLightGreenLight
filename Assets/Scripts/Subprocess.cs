using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Pipes;
using System.IO;

namespace Assets.Scripts
{
    internal class Subprocess : IDisposable
    {
        NamedPipeClientStream pipeClient;
        byte[] buffer;
        Queue<byte[]> messagesFromParent = new Queue<byte[]>();

        public Subprocess(string pipeName)
        {
            Init(pipeName);
        }

        void LogMsg(string msg)
        {
            using (var fs = new StreamWriter("ski.log", true))
            {
                fs.WriteLine(string.Format("{0} {1}", System.DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"), msg));
            }
        }
        public void Dispose() {
            // From the perspective of the main thread (Which should run Dispose)
            // there is always an outstanding read, because Update will finish
            // up the last one and immediately start another.

            // The read is usually not completed, because we're waiting for the next
            // heartbeat from VsKinectPlugin
            if (outstandingRead != null)
            {
                LogMsg("Waiting for IPC read to finish...");
                outstandingRead.AsyncWaitHandle.WaitOne(5 * 1000);
                outstandingRead.AsyncWaitHandle.Close();
                pipeClient.EndRead(outstandingRead);
                outstandingRead = null;
                LogMsg("IPC read finished.");
            }

            if (pipeClient != null)
            {
                LogMsg("Flushing pipeClient...");
                pipeClient.Flush();
                
                pipeClient = null;
            }
        }

        public byte[] DequeueMessage()
        {
            if (messagesFromParent.Any())
            {
                return messagesFromParent.Dequeue();
            }
            else
            {
                return null;
            }
        }

        void Init(string pipeName)
        {
            pipeClient = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
            //pipeClient.ReadTimeout = 0;

            buffer = new byte[1024 * 1024];

            //argLabel.Text = "Waiting on connect...";
            try
            {
                pipeClient.Connect();
            }
            catch (Exception)
            {
                // Ostrich algorithm
            }
            //argLabel.Text = "Waiting on read...";
        }

        public void Write(byte[] msg)
        {
            pipeClient.Write(msg, 0, msg.Length);
        }

        public void Read()
        {
            if (!pipeClient.IsConnected)
            {
                return;
            }

            var bytesRead = pipeClient.Read(buffer, 0, buffer.Length);
            if (bytesRead > 0)
            {
                messagesFromParent.Enqueue(buffer.Take(bytesRead).ToArray());
                //buffer = new byte [buffer.Length];
            }
            else
            {

            }
        }

        static void OnPipeRead(IAsyncResult rc)
        {

        }

        IAsyncResult outstandingRead = null;

        // Update shouldn't be called while Dispose is happening
        // If Update is called from the main thread this shouldn't be a big deal
        public bool Update()
        {
            if (pipeClient == null || !pipeClient.IsConnected)
            {
                return false;
            }

            // Can only read one packet per frame, but hopefully crashes less
            if (outstandingRead != null)
            {
                if (outstandingRead.IsCompleted)
                {
                    var bytesRead = pipeClient.EndRead(outstandingRead);
                    if (bytesRead > 0)
                    {
                        messagesFromParent.Enqueue(buffer.Take(bytesRead).ToArray());
                        //buffer = new byte [buffer.Length];
                    }

                    outstandingRead = pipeClient.BeginRead(buffer, 0, buffer.Length, new AsyncCallback(OnPipeRead), null);
                }
                else
                {
                    // Wait for next frame for the read to finish
                }
            }
            else
            {
                outstandingRead = pipeClient.BeginRead(buffer, 0, buffer.Length, new AsyncCallback(OnPipeRead), null);
            }

            return messagesFromParent.Any();
        }
    }
}
