using System;
using System.Windows;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using SunRise.CyberLock.Common.Library.Data;
using SunRise.CyberLock.ServerSide.DAL;

namespace SunRise.CyberLock.ServerSide.UI.SessionEdition
{
    public class PaymentEditionView : SessionManager
    {
        private Boolean isDifferentSession = false;
        private Double additionPayment = 0;

        public PaymentEditionView()
        {
            this.Title = "Добавление платежа";
            this.label_Information.Content = "К текущему сеансу будет добавлена выбранная сумма.";
            this.label_Information.Background = (System.Windows.Media.Brush)(new System.Windows.Media.BrushConverter()).ConvertFrom("#FFF7FF58");
            btn_OK.Content = "Продлить сеанс";
            EndTimeGrid.IsEnabled = false;
            tariffGrid.IsEnabled = false;
            RemainingTime_Grid.IsEnabled = false;
            checkBox_IsToKillProcesses.Visibility = Visibility.Hidden;
            tb_SessionExpire.SetBinding(System.Windows.Controls.TextBox.TextProperty, "00:00");
            tb_Amount.Focus();
        }

        public void View(IList selectedClients, SessionTariff tariff, Boolean isInternet)
        {
            ClientsList = selectedClients;
            if (!PriceList.Contains(tariff))
                PriceList.Add(tariff);
            SelectedTariff = tariff;
            IsInternetSession = isInternet;

            this.InitDataContext();
            this.ShowDialog();
        }

        public void View(IList selectedClients)
        {
            this.isDifferentSession = true;
            sliderHGrid.IsEnabled = false;
            sliderMGrid.IsEnabled = false;
            ClientsList = selectedClients;

            var newTarrif = new SessionTariff() { Name = "== Текущий ==", LimitedTimeMode = true, CostPerHourGame = 13, CostPerHourInternet = 13 };
            PriceList.Add(newTarrif);
            SelectedTariff = newTarrif;

            this.InitDataContext();
            this.ShowDialog();
        }

        protected override void btn_OK_Click(object sender, RoutedEventArgs e)
        {
            foreach (Client client in ClientsList)
            {
                client.ToAddPayment_Session(this.Payment);
            }
            this.Close();
        }

        #region Properties
        public override Double Payment
        {
            set
            {
                if (!isDifferentSession)
                {
                    this.SetTotalMinutes(value / this.CostPerHour * 60.0);
                    this.PropertyChangedEvent("TotalMinutes");
                    this.PropertyChangedEvent("RemainingHours");
                    this.PropertyChangedEvent("RemainingMinutes");
                }
                else
                {
                    this.additionPayment = value;
                }
            }
            get
            {

                if (!isDifferentSession)
                {
                    return base.Payment;
                }
                else
                {
                    return this.additionPayment;
                }
            }
        }
        #endregion

    }
}
