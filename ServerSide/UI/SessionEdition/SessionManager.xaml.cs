using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Collections;
using System.ComponentModel;
using System.Collections.ObjectModel;
using SunRise.CyberLock.Common.Library.Data;
using SunRise.CyberLock.ServerSide.DAL;

namespace SunRise.CyberLock.ServerSide.UI.SessionEdition
{
    /// <summary>
    /// Interaction logic for SessionManager.xaml
    /// </summary>
    public partial class SessionManager : Window, INotifyPropertyChanged
    {
        private const int maxHour = 15;
        private const int maxMin = 59;
        protected const int maxMinutesCount = maxHour * 60 + maxMin;
        protected const double maxHoursCount = maxMinutesCount / 60.0;

        public ObservableCollection<SessionTariff> PriceList { get; protected set; }
        protected IList ClientsList;

        protected SessionTariff selectedTariff;
        //private Boolean isGamingSession = true;
        private Boolean isInternetSession = false;
        private Boolean toKillProcesses = false;
        protected Double totalMinutes = 0;
        
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        public void PropertyChangedEvent(String propertyName)
        {
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public SessionManager()
        {
            LoadPriceList();
            InitializeComponent();
            sliderHours.Maximum = maxHour;
            sliderMinutes.Maximum = maxMin;
        }

        protected virtual void LoadPriceList()
        {
            PriceList = SessionTariffHelper.LoadPriceFromXML(Constants.GetPricePath());
        }

        protected void InitDataContext()
        {
            this.DataContext = this;
        }


        #region Fields
        protected virtual int MaxMinutesCount
        {
            get { return maxMinutesCount; }
        }

        public virtual SessionTariff SelectedTariff
        {
            get { return this.selectedTariff; }
            set
            {
                selectedTariff = value;
                this.PropertyChangedEvent("IsLimited");
                this.PropertyChangedEvent("TotalMinutes");
                this.PropertyChangedEvent("RemainingHours");
                this.PropertyChangedEvent("RemainingMinutes");
                this.PropertyChanged_Payment();
                this.PropertyChangedEvent("SessionExpireTime");
                this.tb_Amount.Focus();
            }
        }

        public Double CostPerHour
        {
            get
            {
                if (this.SelectedTariff != null && !this.SelectedTariff.LimitedTimeMode)
                    return 0;
                else
                    return this.IsInternetSession ? this.SelectedTariff.CostPerHourInternet : this.SelectedTariff.CostPerHourGame;
            }
        }

        //public Boolean IsGamingSession
        //{
        //    get { return isGamingSession; }
        //    set { isGamingSession = value; }
        //}

        public Boolean IsInternetSession
        {
            get { return isInternetSession; }
            set { isInternetSession = value; }
        }

        public Boolean ToKillProcesses
        {
            get { return toKillProcesses; }
            set { 
                toKillProcesses = value;
                tb_Amount.Focus();
            }
        }

        public virtual Double TotalMinutes
        {
            get { return this.totalMinutes; }
            set
            {
                this.SetTotalMinutes(value);
                this.PropertyChangedEvent("RemainingHours");
                this.PropertyChangedEvent("RemainingMinutes");
                this.PropertyChanged_Payment();
                this.PropertyChangedEvent("SessionExpireTime");
            }
        }

        public virtual bool IsLimited
        {
            get { return this.SelectedTariff.LimitedTimeMode; }
        }

        public virtual Double Payment
        {
            set
            {
                this.SetTotalMinutes(value / this.CostPerHour * 60.0);
                this.PropertyChangedEvent("TotalMinutes");
                this.PropertyChangedEvent("RemainingHours");
                this.PropertyChangedEvent("RemainingMinutes");
                this.PropertyChangedEvent("SessionExpireTime");
            }
            get
            {
                if (this.IsLimited)
                    return this.CostPerHour * this.TotalMinutes / 60.0;
                else return 1;
            }
        }
        
        public virtual DateTime SessionExpireTime
        {
            get { return this.IsLimited ? DateTime.Now.AddHours(this.Payment / this.CostPerHour) : DateTime.Now.AddHours(23); }
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

        public virtual Double RemainingHours
        {
            get { return Math.Floor(TotalMinutes / 60); }
            set
            {
                TotalMinutes = (TotalMinutes % 60) + value * 60;
                this.PropertyChangedEvent("RemainingHours");
                this.PropertyChangedEvent("RemainingMinutes");
                this.PropertyChangedEvent("SessionExpireTime");
                this.PropertyChanged_Payment();
            }
        }

        public virtual Double RemainingMinutes
        {
            get { return TotalMinutes % 60; }
            set
            {
                var tm = TotalMinutes;
                TotalMinutes = tm - (tm % 60) + value;
                this.PropertyChangedEvent("RemainingHours");
                this.PropertyChangedEvent("RemainingMinutes");
                this.PropertyChangedEvent("SessionExpireTime");
                this.PropertyChanged_Payment();
            }
        }
        #endregion

        protected virtual void SetTotalMinutes(Double value)
        {
            if (value <= MaxMinutesCount)
                this.totalMinutes = value;
            else
                this.totalMinutes = MaxMinutesCount;
        }

        protected virtual void PropertyChanged_Payment()
        {
            this.PropertyChangedEvent("Payment");
        }

        protected virtual void btn_OK_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        void TimeTextChanged(TextBox textBox, int minValue, int maxValue)
        {
            int value = 0;
            if (Int32.TryParse(textBox.Text, out value))
            {
                if (value > maxValue)
                {
                    textBox.Text = maxValue.ToString();
                }
                else if (value < minValue)
                {
                    textBox.Text = minValue.ToString();
                }
            }
            else
            {
                textBox.Text = DateTime.Now.Hour.ToString();
            }
            BindingExpression be = textBox.GetBindingExpression(TextBox.TextProperty);
            be.UpdateSource();
        }

        private void tb_SessionExpire_TextChanged(object sender, TextChangedEventArgs e)
        {
            var time = DateTime.Now;
            TextBox textBox = (TextBox)sender;
            if (DateTime.TryParse(textBox.Text, out time))
            {
                if (textBox.Text.Length == 5)
                {
                    BindingExpression be = textBox.GetBindingExpression(TextBox.TextProperty);
                    be.UpdateSource();
                }
            }
            else
            {
                this.PropertyChangedEvent("SessionExpireTime");
            }
        }

        private void tb_Hour_TextChanged(object sender, TextChangedEventArgs e)
        {
            TimeTextChanged((TextBox)sender, 0, 23);
        }

        private void tb_Minute_TextChanged(object sender, TextChangedEventArgs e)
        {
            TimeTextChanged((TextBox)sender, 0, 59);
        }

        private void tb_Amount_TextChanged(object sender, TextChangedEventArgs e)
        {
            BindingExpression be = (sender as TextBox).GetBindingExpression(TextBox.TextProperty);
            be.UpdateSource();
        }

        private void control_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                btn_OK_Click(this, new RoutedEventArgs());
        }

    }

}