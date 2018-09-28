using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using SunRise.CyberLock.Common.Library.Data;
using SunRise.CyberLock.ServerSide.DAL;

namespace SunRise.CyberLock.ServerSide.UI.SessionEdition
{
    class SessionExtensionView : SessionManager
    {
        private DateTime currentExpireDate;
        private Double currentPayment;

        public SessionExtensionView()
        {
            btn_OK.Content = "Изменить сеанс";
            this.Title = "Изменение сеанса";
            this.label_Information.Content = "Сеанс будет изменен.";
            this.label_Information.Background = (System.Windows.Media.Brush)(new System.Windows.Media.BrushConverter()).ConvertFrom("#FF5D89FF");
            
            sliderHours.SetBinding(Slider.SelectionEndProperty, new Binding("CurrentHours") { Mode = BindingMode.OneWay });
            sliderMinutes.SetBinding(Slider.SelectionEndProperty, new Binding("CurrentMinutes") { Mode = BindingMode.OneWay });
            tb_CurrentPayment.SetBinding(TextBox.TextProperty, new Binding("CurrentPayment") { Mode = BindingMode.OneWay, StringFormat = "0.00" });
            tb_TotalPayment.SetBinding(TextBox.TextProperty, new Binding("TotalPayment") { Mode = BindingMode.OneWay, StringFormat = "0.00" });
           
            sliderHours.IsSelectionRangeEnabled = true;
            sliderMinutes.IsSelectionRangeEnabled = true;
            checkBox_IsToKillProcesses.Visibility = Visibility.Hidden;
            tb_Amount.Focus();
            this.InitDataContext();
        }

        public void View(IList selectedClients, DateTime expireDate, SessionTariff tariff, Double payment)
        {
            this.currentExpireDate = expireDate;
            this.CurrentPayment = payment;
            this.ClientsList = selectedClients;
            this.SelectedTariff = tariff;

            this.ShowDialog();
        }

        #region Properies
        protected override int MaxMinutesCount
        {
            get { return maxMinutesCount - (int)this.CurrentTotalMinutes; }
        }

        public Double CurrentTotalMinutes
        {
            get { return Math.Round((this.currentExpireDate - DateTime.Now).TotalMinutes); }
        }

        public DateTime CurrentExpireDate
        {
            get { return this.currentExpireDate; }
            set
            {
                this.currentExpireDate = value;
                this.PropertyChangedEvent("CurrentHours");
                this.PropertyChangedEvent("CurrentMinutes");
            }
        }

        public Double CurrentHours
        {
            get
            {
                return Math.Floor((currentExpireDate - DateTime.Now).TotalMinutes / 60);
            }
        }

        public Double CurrentMinutes
        {
            get
            {
                return (currentExpireDate - DateTime.Now).TotalMinutes % 60;
            }
        }

        public Double CurrentPayment
        {
            get
            {
                return this.currentPayment;
            }
            private set
            {
                this.currentPayment = value;
                this.PropertyChangedEvent("CurrentPayment");
            }
        }

        public Double TotalPayment
        {
            get
            {
                return this.CurrentPayment + this.Payment;
            }
        }

        public override double TotalMinutes
        {
            get
            {
                return base.TotalMinutes + this.CurrentTotalMinutes;
            }
            set
            {
                base.TotalMinutes = value - this.CurrentTotalMinutes;
            }
        }

        public override double Payment
        {
            get
            {
                if (this.IsLimited)
                    return this.CostPerHour * (base.TotalMinutes) / 60.0;
                else return 0;
            }
            set
            {
                base.Payment = value;
                this.PropertyChangedEvent("TotalPayment");
            }
        }

        public override SessionTariff SelectedTariff
        {
            get { return base.SelectedTariff; }
            set
            {
                if (value.LimitedTimeMode && this.SelectedTariff != null && this.SelectedTariff.LimitedTimeMode)
                {
                    var tariff = this.SelectedTariff;
                    var restPayment = (this.CurrentExpireDate - DateTime.Now).TotalHours * (this.IsInternetSession ? tariff.CostPerHourInternet : tariff.CostPerHourGame);
                    this.CurrentExpireDate = DateTime.Now.AddHours(restPayment / (this.IsInternetSession ? value.CostPerHourInternet : value.CostPerHourGame));
                }
                base.SelectedTariff = value;
            }
        }

        public override DateTime SessionExpireTime
        {
            get { return (this.IsLimited ? this.CurrentExpireDate.AddHours(this.Payment / this.CostPerHour) : DateTime.Now.AddHours(23)); }
            set
            {
                if (value.CompareTo(DateTime.Now) < 0)
                {
                    value = value.AddDays(1);
                }
                var minutes = (value - DateTime.Now).TotalMinutes;
                if (minutes <= maxMinutesCount)
                {
                    this.SetTotalMinutes(minutes);
                    this.PropertyChangedEvent("RemainingHours");
                    this.PropertyChangedEvent("RemainingMinutes");
                    this.PropertyChangedEvent("SessionExpireTime");
                    this.PropertyChanged_Payment();
                }
                this.PropertyChangedEvent("SessionExpireTime");
            }
        }
        #endregion

        protected override void PropertyChanged_Payment()
        {
            base.PropertyChanged_Payment();
            this.PropertyChangedEvent("TotalPayment");
        }

        protected override void btn_OK_Click(object sender, RoutedEventArgs e)
        {
            foreach (Client client in ClientsList)
            {
                client.ToExtend_Session(this.SelectedTariff, this.IsInternetSession, this.TotalPayment, this.SessionExpireTime);
            }
            this.Close();
        }
    }
}
