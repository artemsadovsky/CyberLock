using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Collections.ObjectModel;
using SunRise.CyberLock.ClientSide.BL;
using SunRise.CyberLock.ClientSide.Utils.TaskbarNotifier;
using System.Windows.Threading;

namespace SunRise.CyberLock.ClientSide.UI
{
    public class NotifyObject
    {
        public NotifyObject(string message, string title)
        {
            this.message = message;
            this.title = title;
        }

        private string title;
        public string Title
        {
            get { return this.title; }
            set { this.title = value; }
        }

        private string message;
        public string Message
        {
            get { return this.message; }
            set { this.message = value; }
        }
    }

    public partial class TaskbarNotifierWindow : TaskbarNotifier
    {
        private DispatcherTimer timer;
        private TaskbarNotifierHandler _handler;

        public TaskbarNotifierWindow(TaskbarNotifierHandler handler)
        {
            InitializeComponent();
            this._handler = handler;
        }

        public void ShowRemainingTime()
        {
            //notifyContent.Add(new NotifyObject("message", "title"));
            lb_RemainingTime.Content = this._handler.GetRemainingTime();
            if (timer == null)
            {
                timer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(1) };
                timer.Tick += new EventHandler(delegate(object s, EventArgs a)
                {
                    lb_RemainingTime.Content = this._handler.GetRemainingTime();

                    if (this.DisplayState == DisplayStates.Hidden)
                    {
                        timer.Stop();
                    }
                });
            }
            if (!timer.IsEnabled)
                timer.Start();
            base.Notify();
        }

        public void ShowInformation(String information)
        {
            //notifyContent.Add(new NotifyObject("message", "title"));
            if (timer != null && timer.IsEnabled)
            {
                timer.Stop();
                this.ForceHidden();
            }
            lb_RemainingTime.Content = information;
            timer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(1) };
            timer.Tick += new EventHandler(delegate(object s, EventArgs a)
            {
                if (this.DisplayState == DisplayStates.Hidden)
                {
                    timer.Stop();
                }
            });

            timer.Start();
            base.Notify();
        }

        private ObservableCollection<NotifyObject> notifyContent;
        /// <summary>
        /// A collection of NotifyObjects that the main window can add to.
        /// </summary>
        public ObservableCollection<NotifyObject> NotifyContent
        {
            get
            {
                if (this.notifyContent == null)
                {
                    // Not yet created.
                    // Create it.
                    this.NotifyContent = new ObservableCollection<NotifyObject>();
                }

                return this.notifyContent;
            }
            set
            {
                this.notifyContent = value;
            }
        }

        private void Item_Click(object sender, EventArgs e)
        {
            Hyperlink hyperlink = sender as Hyperlink;

            if (hyperlink == null)
                return;

            NotifyObject notifyObject = hyperlink.Tag as NotifyObject;
            if (notifyObject != null)
            {
                MessageBox.Show("\"" + notifyObject.Message + "\"" + " clicked!");
            }
        }

        private void HideButton_Click(object sender, EventArgs e)
        {
            this.ForceHidden();
        }
    }
}