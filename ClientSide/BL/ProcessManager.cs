using System;
using System.Management;
using System.Diagnostics;
using System.Security.Principal;
using System.Linq;
using System.Runtime.InteropServices;

namespace SunRise.CyberLock.ClientSide.BL
{
    public class ProcessManager
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);



        #region System Shutdown method
        /// <summary>
        ///  Shutdown,restart and log off Computer
        /// </summary>
        /// <param name="Flags">Specify the Shut down Parameter
        ///  "1" - Shut down
        ///  "2" - Restart
        ///  "0" - Log off
        /// </param>
        public static void ShutDownWindows(String Flags)
        {
            try
            {
                ManagementBaseObject MBOShutdown = null;
                ManagementClass MCWin32 = new ManagementClass("Win32_OperatingSystem");
                MCWin32.Get();


                MCWin32.Scope.Options.EnablePrivileges = true;
                ManagementBaseObject MBOShutdownParams = MCWin32.GetMethodParameters("Win32Shutdown");

                MBOShutdownParams["Flags"] = Flags;
                MBOShutdownParams["Reserved"] = "0";

                foreach (ManagementObject manObj in MCWin32.GetInstances())
                {
                    MBOShutdown = manObj.InvokeMethod("Win32Shutdown", MBOShutdownParams, null);
                }
            }
            catch (Exception ex)
            {
                Locker.AppLogger.Error("ShutDownWindows()", "RunMode is " + Flags + ". Exception: " + ex.Message);
            }
        }
        #endregion

        #region Tasks Killer
        private static void KillProcess(ManagementObject managementObject, Process process, string userName)
        {
            try
            {
                var processOwnerInfo = new object[2];
                managementObject.InvokeMethod("GetOwner", processOwnerInfo);

                var processOwner = (string)processOwnerInfo[0];
                var net = (string)processOwnerInfo[1];


                if (!string.IsNullOrEmpty(net))
                    processOwner = string.Format("{0}\\{1}", net, processOwner);

                //Logger.Logger.WriteLog(process.ProcessName + " (" + processOwner + ")", "procUtil.log");
                if (string.CompareOrdinal(processOwner, userName) == 0)
                    process.Kill();
            }
            catch (Exception ex)
            {
                Locker.AppLogger.Error("KillProcess()", "Process name: " + process.ProcessName + ". Exception: " + ex.Message);
            }
        }

        public static void KillProcessByName(String processName)
        {
            try
            {
                var processes = Process.GetProcessesByName(processName);
                foreach (Process proc in processes)
                {
                    proc.Kill();
                }
            }
            catch (Exception ex)
            {
                Locker.AppLogger.Error("KillProcessByName()", "Process name: " + processName + ". Exception: " + ex.Message);
            }
        }

        public static void KillAllProcesses(string[] includes = null, string[] exclusions = null, bool currentUserOnly = true)
        {
            try
            {
                string userName = null;
                var currentProcessId = Process.GetCurrentProcess().Id;
                if (currentUserOnly)
                {
                    WindowsIdentity user = WindowsIdentity.GetCurrent();

                    userName = user.Name;
                }

                //var processFinder = new ManagementObjectSearcher(string.Format("Select * from Win32_Process where Name='{0}'", processName));
                var processFinder = new ManagementObjectSearcher("Select * from Win32_Process");
                var processes = processFinder.Get();

                if (processes.Count == 0)
                    return;

                foreach (ManagementObject managementObject in processes)
                {
                    try
                    {
                        var pId = Convert.ToInt32(managementObject["ProcessId"]);
                        if (pId != currentProcessId)
                        {
                            var process = Process.GetProcessById(pId);
                            if (exclusions == null || !exclusions.Any(p => p.Equals(process.ProcessName, System.StringComparison.CurrentCultureIgnoreCase)))
                            {
                                //Logger.Logger.WriteLog(process.ProcessName, "procUtil.log");
                                if ((includes != null && includes.Any(p => p.Equals(process.ProcessName, System.StringComparison.CurrentCultureIgnoreCase)))
                                    || !currentUserOnly) //any user
                                    process.Kill();
                                else //current user
                                    KillProcess(managementObject, process, userName);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Locker.AppLogger.Error("KillAllProcesses()", "foreach section. Exception: " + ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                Locker.AppLogger.Error("KillAllProcesses()", "Exception: " + ex.Message);
            }
        }
        #endregion

        #region Process management
        public static Boolean IsForegrount()
        {
            IntPtr hWnd = GetForegroundWindow();
            int pid;
            //получаем pid потока активного окна
            GetWindowThreadProcessId(hWnd, out pid);

            var currentProcess = Process.GetCurrentProcess();

            return currentProcess.Id == pid;
        }

        #endregion
    }

}

