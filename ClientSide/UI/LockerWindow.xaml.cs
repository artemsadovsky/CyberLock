using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using System.Windows.Input;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Management;
using SunRise.CyberLock.ClientSide.BL;

namespace SunRise.CyberLock.ClientSide.UI
{
    /// <summary>
    /// Interaction logic for LockerWindow.xaml
    /// </summary>
    public partial class LockerWindow : SunRise.CyberLock.ClientSide.BL.Locker
    {
        private TaskbarNotifierWindow taskbarNotifier;

        public LockerWindow()
        {
            InitializeComponent();

            //mapping components
            base.BigInformer = this.lb_bInform;
            base.MiddleInformer = this.lb_mInform;
            base.SmallInformer = this.lb_sInform;
            base.ErrorInformer = this.lb_eInform;
            base.GridFinisher = this.gridFinisher;
            base.PasswordModeInformer = this.lb_password;

            this.Background = System.Windows.Media.Brushes.Black;
            this.Loaded += this.OnLoaded;
            this.Closing += this.OnClosing;
        }


        private void NotifyIconOpen_Click(object sender, RoutedEventArgs e)
        {
            this.taskbarNotifier.ShowRemainingTime();
        }

        protected void OnLoaded(object sender, RoutedEventArgs e)
        {
            base.Locker_Loaded();

            this.taskbarNotifier = new TaskbarNotifierWindow(base._taskbarNotifierHandler) { ShowInTaskbar = false, WindowStyle = System.Windows.WindowStyle.None, Owner = this };
            this.taskbarNotifier.Show();
            this.taskbarNotifier.ShowRemainingTime();
            this.taskbarNotifier.ForceHidden();
        }

        private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //logger
            if (base.reallyCloseWindow)
            {
                this.NotifyIcon.Visibility = Visibility.Collapsed;

                // Close the taskbar notifier too.
                if (this.taskbarNotifier != null)
                    this.taskbarNotifier.Close();

                //Call parent event
                base.Locker_Closing();
            }
            else
                e.Cancel = true;
        }

    }
}
