using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;
using SunRise.CyberLock.ClientSide.Settings.SLib;

namespace SunRise.CyberLock.ClientSide.Utils.Inspector
{
    class Program
    {
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;
        const String PROC_NAME = "Locker";
        static String PROC_PATH;

        private static bool _quitRequested = false;
        private static object _syncLock = new object();
        private static AutoResetEvent _waitHandle = new AutoResetEvent(false);

        static void Main(string[] args)
        {
            HideApp();
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(CurrentDomain_ProcessExit);
            PROC_PATH = (new Configuration()).FIELDS.App_Folder + "Locker.exe";

            Thread mainThread = new Thread(Watcher);
            mainThread.Start();
            
            // read input to detect "quit" command
            string command = string.Empty;
            do
            {
                command = Console.ReadLine();
            } while (!command.Equals("quit", StringComparison.InvariantCultureIgnoreCase));
            
            // signal that we want to quit
            SetQuitRequested();
            
            // wait until the thread says it's done
            _waitHandle.WaitOne();
        }        

        private static void HideApp()
        {
            var handle = GetConsoleWindow();
            ShowWindow(handle, SW_HIDE);
            //System.Threading.Thread.Sleep(3000);
            //ShowWindow(handle, SW_SHOW);
            //System.Threading.Thread.Sleep(3000);
            //ShowWindow(handle, SW_HIDE);
        }

        private static void SetQuitRequested()
        {
            lock (_syncLock)
            {
                _quitRequested = true;
            }
        }

        private static void Watcher()
        {
            do
            {
                try
                {
                    if (!ProcessIsRunning())
                    {
                        LaunchMainApp();
                    }
                }
                catch
                {
                    //
                }
                finally
                {
                    Thread.Sleep(5000);
                }
            } while (!_quitRequested);
            _waitHandle.Set();
        }

        private static bool ProcessIsRunning()
        {
            return (Process.GetProcessesByName(PROC_NAME).Length != 0);
        }

        private static void LaunchMainApp()
        {
            Process pr = new Process();
            pr.StartInfo.FileName = PROC_PATH;
            pr.Start();
        }

        static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            Console.WriteLine("quit");
        }
    }
}
