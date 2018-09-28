using System;
using System.Windows;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using SunRise.CyberLock.Common.Library.Data;
using SunRise.CyberLock.ServerSide.DAL;
using System.Windows.Threading;

namespace SunRise.CyberLock.ServerSide.UI.SessionEdition
{
    class SessionCreationView : SessionManager
    {
        public SessionCreationView()
        {
            this.btn_OK.Content = "Создать сеанс";
            this.Title = "Создание нового сеанса";
            this.label_Information.Content = "Будет создан новый сеанс.";
            this.label_Information.Background = (System.Windows.Media.Brush)(new System.Windows.Media.BrushConverter()).ConvertFrom("#FF6FFF58");
            this.tb_Amount.Focus();
            this.InitDataContext();
        }

        public void View(IList selectedClients)
        {
            ClientsList = selectedClients;
            if (PriceList.Count > 0) SelectedTariff = PriceList[0];
            this.ShowDialog();
        }

        protected override void btn_OK_Click(object sender, RoutedEventArgs e)
        {
            var timeNow = DateTime.Now;
            var newSession = new SessionMessage(timeNow, this.IsLimited ? timeNow.AddHours(this.Payment / this.CostPerHour) : timeNow.AddHours(23), this.SelectedTariff, this.IsInternetSession, this.Payment);
            foreach (Client client in ClientsList)
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, (DispatcherOperationCallback)delegate(object o)
                {
                    client.ToCreate_Session(newSession, ToKillProcesses);
                    return null;
                }, null);
            }
            this.Close();
        }
    }
}

