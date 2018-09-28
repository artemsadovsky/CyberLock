using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.IO;
using System.Windows.Threading;

namespace SunRise.CyberLock.ClientSide.Locker
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly String CONFIG_LOG_PATH = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)
                                                                       + @"\SunRise\CyberLock-Client\logs";
        private static readonly String HIGH_LOG_FILE = "high.log";
        private static readonly String FULL_LOG_PATH = CONFIG_LOG_PATH + @"\" + HIGH_LOG_FILE;

        protected override void OnStartup(StartupEventArgs e)
        {
            String destinationFolder = @"c:\progs\CyberLock";
            String sourceFolder = @"c:\progs\CyberLock\Update";
            if (Directory.Exists(sourceFolder))
            {
                var files = Directory.EnumerateFiles(sourceFolder);
                foreach (var sourceFile in files)
                {
                    var list = sourceFile.Split('\\');
                    var destinationFile = destinationFolder + "\\" + list[list.Count()-1];

                    if (File.Exists(destinationFile)) File.Delete(destinationFile);
                    File.Move(sourceFile, destinationFile);
                }

            }
            //AppDomain.CurrentDomain.UnhandledException += (s, ex) =>
            //{
            //    //System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);

            //    base.Shutdown();
            //};

            base.OnStartup(e);
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            //Handling the exception within the UnhandledExcpeiton handler.
            #if DEBUG
            MessageBoxResult res = MessageBox.Show(e.Exception.Message, "Exception Caught", MessageBoxButton.OKCancel, MessageBoxImage.Error);
            WiteErrorLog(e.Exception.Message);
            e.Handled = true;
            if(res.CompareTo(MessageBoxResult.OK) == 0)
                base.Shutdown();
            #else
            WiteErrorLog(e.Exception.Message);
            e.Handled = true;
            base.Shutdown();
            #endif
        }

        private void WiteErrorLog(String message)
        {
            if (!Directory.Exists(CONFIG_LOG_PATH))
            {
                Directory.CreateDirectory(CONFIG_LOG_PATH);
            }
            if (!File.Exists(HIGH_LOG_FILE))
            {
                var fileSteam = File.Create(HIGH_LOG_FILE);
                using (StreamWriter writer = new StreamWriter(fileSteam))
                {
                    writer.Close();
                }
            }

            var sw = File.AppendText(FULL_LOG_PATH);
            sw.WriteLine(String.Format("{0} [ERROR] : {1}", DateTime.Now, message));
            sw.Close();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
        }
    }
}
