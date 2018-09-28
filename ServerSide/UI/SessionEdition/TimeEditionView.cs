using System;
using System.Windows;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using SunRise.CyberLock.Common.Library.Data;
using SunRise.CyberLock.ServerSide.DAL;

namespace SunRise.CyberLock.ServerSide.UI.SessionEdition
{
    public class TimeEditionView : SessionManager
    {
        public TimeEditionView()
        {
            this.Title = "Добавление времени";
            this.label_Information.Content = "К текущему сеансу будет добавлено выбранное время.";
            this.label_Information.Background = (System.Windows.Media.Brush)(new System.Windows.Media.BrushConverter()).ConvertFrom("#FF66D85B");
            btn_OK.Content = "Продлить сеанс";
            PaymentGrid.IsEnabled = false;
            EndTimeGrid.IsEnabled = false;
            tariffGrid.IsEnabled = false;
            RemainingTime_Grid.IsEnabled = false;
            checkBox_IsToKillProcesses.Visibility = Visibility.Hidden;
            tb_Amount.Focus();
        }

        protected override void LoadPriceList()
        {
            this.PriceList = new ObservableCollection<SessionTariff> { new SessionTariff() { Name = "== Текущий ==", LimitedTimeMode = true, CostPerHourGame = 0, CostPerHourInternet = 0 } };
        }

        public void View(IList selectedClients)
        {
            ClientsList = selectedClients;
            SelectedTariff = this.PriceList[0];
            this.InitDataContext();
            this.ShowDialog();
        }

        #region Properties
        public override bool IsLimited
        {
            get { return this.SelectedTariff.CostPerHourGame == 13 && this.SelectedTariff.CostPerHourInternet == 13 ? true : this.SelectedTariff.LimitedTimeMode; }
        }

        public override double TotalMinutes
        {
            get { return base.TotalMinutes; }
            set
            {
                this.SetTotalMinutes(value);
                this.PropertyChangedEvent("RemainingHours");
                this.PropertyChangedEvent("RemainingMinutes");
            }
        }

        public override double RemainingHours
        {
            get
            {
                return base.RemainingHours;
            }
            set
            {
                TotalMinutes = (TotalMinutes % 60) + value * 60;
                this.PropertyChangedEvent("RemainingHours");
                this.PropertyChangedEvent("RemainingMinutes");
            }
        }

        public override double RemainingMinutes
        {
            get
            {
                return base.RemainingMinutes;
            }
            set
            {
                var tm = TotalMinutes;
                TotalMinutes = tm - (tm % 60) + value;
                this.PropertyChangedEvent("RemainingHours");
                this.PropertyChangedEvent("RemainingMinutes");
            }
        }
        #endregion

        protected override void btn_OK_Click(object sender, RoutedEventArgs e)
        {
            foreach (Client client in ClientsList)
            {
                client.ToAddTime_Session(this.TotalMinutes);
            }
            this.Close();
        }

    }
}

