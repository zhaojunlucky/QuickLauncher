using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Reflection;
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

            try
            {
                namedPipeServerStream.BeginWaitForConnection(asyncCallback, namedPipeServerStream);
            }
            catch (ObjectDisposedException)
            {
                // Pipe was disposed before connection could be made, ignore or log as needed
                Trace.TraceWarning("Pipe server disposed before connection.");
            }
            catch (IOException ioEx)
            {
                // Handle pipe broken or other IO errors
                Trace.TraceError("Pipe server IO error: " + ioEx.Message);
                // Optionally, recreate the pipe server and try again
                namedPipeServerStream?.Dispose();
                namedPipeServerStream = new NamedPipeServerStream(AppName + "IPC",
                    PipeDirection.InOut, 1, PipeTransmissionMode.Message, PipeOptions.Asynchronous);
                namedPipeServerStream.BeginWaitForConnection(asyncCallback, namedPipeServerStream);
            }
        }

        public void ReListen()
        {
            // Dispose the old pipe if it exists
            if (namedPipeServerStream != null)
            {
                try
                {
                    if (namedPipeServerStream.IsConnected)
                    {
                        namedPipeServerStream.Disconnect();
                    }
                }
                catch (IOException ioEx)
                {
                    Trace.TraceWarning("Pipe disconnect failed: " + ioEx.Message);
                }
                namedPipeServerStream.Dispose();
                namedPipeServerStream = null;
            }

            // Create a new pipe server instance
            namedPipeServerStream = new NamedPipeServerStream(AppName + "IPC",
                PipeDirection.InOut, 1, PipeTransmissionMode.Message, PipeOptions.Asynchronous);

            try
            {
                namedPipeServerStream.BeginWaitForConnection(asyncCallback, namedPipeServerStream);
            }
            catch (ObjectDisposedException)
            {
                Trace.TraceWarning("Pipe server disposed before connection.");
            }
            catch (IOException ioEx)
            {
                Trace.TraceError("Pipe server IO error: " + ioEx.Message);
            }
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

        // ...
        public void SendMsgToRunningServer(string message)
        {
            try
            {
                using (var cli = new NamedPipeClientStream(".", AppName + "IPC", PipeDirection.InOut, PipeOptions.None))
                {
                    try
                    {
                        cli.Connect(2000);
                    }
                    catch (TimeoutException)
                    {
                        Trace.TraceError("Timeout: Could not connect to pipe server.");
                        return;
                    }
                    catch (IOException ioEx)
                    {
                        Trace.TraceError("Pipe connection failed: " + ioEx.Message);
                        return;
                    }

                    if (!cli.IsConnected)
                    {
                        Trace.TraceError("Pipe client is not connected.");
                        return;
                    }

                    var bytes = Encoding.UTF8.GetBytes(message);

                    try
                    {
                        cli.Write(bytes, 0, bytes.Length);
                        cli.Flush();
                        cli.WaitForPipeDrain();
                    }
                    catch (IOException ioEx)
                    {
                        Trace.TraceError("Pipe is broken during write: " + ioEx.Message);
                    }
                }
            }
            catch (Exception e)
            {
                Trace.TraceError(e.Message);
                Trace.TraceError(e.StackTrace);
            }
        }
    }
}
