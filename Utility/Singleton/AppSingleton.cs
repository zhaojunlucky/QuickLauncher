using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Utility.Singleton
{
    public class AppSingleton
    {
        private System.Threading.Mutex mutex;
        private readonly object obj = new object();
#if DEBUG
        private static readonly string AppName =
          Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly()?.GetName().Name) + "_DEBUG";
#else
        private static readonly string AppName =
          Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().GetName().Name);
#endif
        private NamedPipeServerStream namedPipeServerStream;
        private AsyncCallback asyncCallback;

        public static AppSingleton Instance { get; } = new AppSingleton();

        private AppSingleton()
        {

        }

        public bool CheckIsAppRunning(string mutexName)
        {
            bool isRunning = false;
            lock (obj)
            {
                if (mutex == null)
                {
                    mutex = new System.Threading.Mutex(true, GetHash(mutexName), out bool createdNew);
                    isRunning = !createdNew;
                }
            }
            return isRunning;
        }

        private string GetHash(string mutexName)
        {
            byte[] data = HashAlgorithm.Create("SHA256")?.ComputeHash(Encoding.UTF8.GetBytes(mutexName));
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data  
            // and format each one as a hexadecimal string. 
            foreach (var t in data)
            {
                sBuilder.Append(t.ToString("x2"));
            }

            // Return the hexadecimal string. 
            return sBuilder.ToString();
        }

        public void StartPipeServer(AsyncCallback asyncCallback)
        {
            this.asyncCallback = asyncCallback;
            namedPipeServerStream = new NamedPipeServerStream(AppName + "IPC",
               PipeDirection.InOut, 1, PipeTransmissionMode.Message, PipeOptions.Asynchronous);
            // it's easier to use the AsyncCallback than it is to use Tasks here:
            // this can't block, so some form of async is a must

            namedPipeServerStream.BeginWaitForConnection(asyncCallback, namedPipeServerStream);

        }

        public void ReListen()
        {
            if (namedPipeServerStream is {IsConnected: true})
            {
                namedPipeServerStream.Disconnect();
            }
            namedPipeServerStream.BeginWaitForConnection(asyncCallback, namedPipeServerStream);
        }

        public async Task StopPipeServerAsync()
        {
            if (namedPipeServerStream != null)
            {
                // let the pipe server to exit
                SendMsgToRunningServer("");

                await Task.Delay(500);
                if (namedPipeServerStream.IsConnected)
                {
                    namedPipeServerStream.Disconnect();
                }
                namedPipeServerStream.Close();
            }
        }

        public void SendMsgToRunningServer(string message)
        {
            try
            {
                var cli = new NamedPipeClientStream(".", AppName + "IPC", PipeDirection.InOut);
                cli.Connect(2000);
                var bf = new BinaryFormatter();
                // serialize and send the command line
                bf.Serialize(cli, message);
                cli.Close();
            }
            catch (Exception e)
            {
                Trace.TraceError(e.Message);
                Trace.TraceError(e.StackTrace);
            }
        }
    }
}
