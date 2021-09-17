using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Utility.Win32.Api;

namespace Utility.Singleton
{
    public class AppSingleton
    {
        private System.Threading.Mutex mutex = null;
        private Object obj = new object();
        private static AppSingleton instance = new AppSingleton();
        public static AppSingleton Instance
        {
            get { return instance; }
        }

        private AppSingleton()
        {

        }

        public bool checkIsAppRunning(string mutexName)
        {
            bool isRunning = false;
            lock (obj)
            {
                if (mutex == null)
                {
                    bool createdNew = false;
                    mutex = new System.Threading.Mutex(true, getHash(mutexName), out createdNew);
                    isRunning = !createdNew;
                }
            }
            return isRunning;
        }

        private string getHash(string mutexName)
        {
            byte[] data = HashAlgorithm.Create("SHA256").ComputeHash(Encoding.UTF8.GetBytes(mutexName));
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data  
            // and format each one as a hexadecimal string. 
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string. 
            return sBuilder.ToString();
        }


        public static Process getRunningInstance()
        {
            Process current = Process.GetCurrentProcess();
            Process[] processes = Process.GetProcessesByName(current.ProcessName);
            //遍历与当前进程名称相同的进程列表 
            foreach (Process process in processes)
            {
                //如果实例已经存在则忽略当前进程 
                if (process.Id != current.Id)
                {
                    //保证要打开的进程同已经存在的进程来自同一文件路径
                    //if (Assembly.GetExecutingAssembly().Location.Replace("/", "\\") == current.MainModule.FileName)
                    try
                    {
                        if (process.MainModule.FileName == current.MainModule.FileName)
                        {
                            //返回已经存在的进程
                            return process;
                        }
                    }
                    catch(Win32Exception e)
                    {
                        Trace.TraceInformation(e.Message);
                        Trace.TraceInformation(e.StackTrace);
                    }
                }
            }
            return null;
        }

        public static void sendRunningInstanceForeground(Process instance)
        {
            var hn = instance.MainWindowHandle;
            var handle = Win32Api.GetWindowHandle(instance.Id, "Quick Launcher");
            Win32Api.ShowWindowAsync(handle, 5);  //调用api函数，正常显示窗口
            Win32Api.SetForegroundWindow(handle); //将窗口放置最前端
            Win32Api.SwitchToThisWindow(handle, true);
            Win32Api.SendMessage(handle, Win32Api.WM_SHOWWINDOW, IntPtr.Zero, new IntPtr(Win32Api.SW_PARENTOPENING));
        }
    }
}
