using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ComponentModel;
using SunRise.CyberLock.Common.Library.Data;
using SunRise.CyberLock.Common.Library.Helper;
using SunRise.CyberLock.Common.Library.IServiceContracts;

namespace SunRise.CyberLock.ServerSide.DAL
{
    public class Client : AbstractSession, INotifyPropertyChanged
    {
        private int id;
        private String name;
        private String ip = "--";
        private object callback;
        private bool toSinchronize = false;

        protected DispatcherTimer _Timer;

        private bool isSessionStarted = false;

        private bool isAlreadyWasConnected = false;

        //private Nullable<Double> remainingTime;
        //private Nullable<DateTime> endSession;

        private void ToStartSession()
        {
            if (!IsSessionStarted)
            {
                _Timer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(1) };
                _Timer.Tick += new EventHandler(delegate(object s, EventArgs a)
                {
                    if (toSinchronize)
                    {
                        SendUpdateMessageToClient(false);
                    }
                    if (Tariff.LimitedTimeMode)
                    {
                        if (IsPaused)
                        {
                            SessionExpire = SessionExpire.Value.AddSeconds(1);
                            NotifyPropertyChanged("RemainingTime");
                            NotifyPropertyChanged("EndSessionTime");
                        }
                        else
                        {
                            NotifyPropertyChanged("RemainingTime");
                            if (SessionExpire.Value <= DateTime.Now) ToEndSession();
                        }
                    }
                    //if(callback!=null) ((IClientContract)callback).SessionTick(remainingTime.HasValue ? remainingTime.Value : 0);
                });
                _Timer.Start();
            }
            IsSessionStarted = true;
        }

        private void RefreshSessionExpire()
        {
            if (!this.SessionStart.HasValue)
            {
                this.SessionStart = DateTime.Now;
            }
            this.SessionExpire = this.SessionStart.Value.AddHours(this.TotalPayment / (this.IsInternetSession ? this.Tariff.CostPerHourInternet : this.Tariff.CostPerHourGame));
            NotifyPropertyChanged("EndSessionTime");
        }

        private void RefreshSessionExpire(DateTime sessionExpire)
        {
            if (!this.SessionStart.HasValue)
            {
                this.SessionStart = DateTime.Now;
            }
            this.SessionExpire = sessionExpire;
            NotifyPropertyChanged("EndSessionTime");
        }

        #region Session actions
        public void ToCreate_Session(SessionMessage newSession, Boolean toKillProcesses)
        {
            ToCreate_Session(false, newSession, toKillProcesses);
        }

        public void ToCreate_Session(bool isSentFromClient, SessionMessage newSession, Boolean toKillProcesses)
        {
            this.SessionExpire = newSession.SessionExpire;
            this.SessionStart = newSession.SessionStart;
            this.Tariff = newSession.Tariff;
            this.IsInternetSession = newSession.IsInternetSession;
            this.IsPaused = newSession.IsPaused;
            this.TotalPayment = newSession.TotalPayment;

            this.ToStartSession(); //START SESSION

            NotifyPropertyChanged("StartSessionTime");
            NotifyPropertyChanged("EndSessionTime");
            NotifyPropertyChanged("RemainingTime");

            if (!isSentFromClient)
                SendUpdateMessageToClient(toKillProcesses);
        }

        public void ToExtend_Session(SessionTariff tariff, Boolean isInternetSession, Double totalPayment, DateTime sessionExpire, Boolean toKillProcesses = false)
        {
            if (IsSessionStarted)
            {
                TotalPayment = totalPayment;
                Tariff = tariff;
                RefreshSessionExpire(sessionExpire);
                NotifyPropertyChanged("RemainingTime");
                SendUpdateMessageToClient(toKillProcesses);
            }
            else
            {
                var timeNow = DateTime.Now;
                ToCreate_Session(new SessionMessage(timeNow, sessionExpire, tariff, isInternetSession, totalPayment), toKillProcesses);
            }
        }

        public void ToAddPayment_Session(Double payment, Boolean toKillProcesses = false)
        {
            if (IsSessionStarted)
            {
                TotalPayment += payment;
                EndSession = EndSession.Value.AddHours(payment / (IsInternetSession ? Tariff.CostPerHourInternet : Tariff.CostPerHourGame));
                SendUpdateMessageToClient(toKillProcesses);
            }
        }

        public void ToAddTime_Session(Double minutes, Boolean toKillProcesses = false)
        {
            if (IsSessionStarted)
            {
                EndSession = EndSession.Value.AddMinutes(minutes);
                SendUpdateMessageToClient(toKillProcesses);
            }
            //else ToCreateSession(DateTime.Now, DateTime.Now.AddMinutes(minutes));
        }

        private void SendUpdateMessageToClient(Boolean toKillProcesses)
        {
            if (callback != null)
                try
                {
                    var sysTime = new SYSTEMTIME();
                    SystemTime.GetSystemTime(ref sysTime);
                    ((IClientContract)callback).SessionUpdated(
                        new SessionMessage(
                            this.SessionStart,
                            this.SessionExpire,
                            this.Tariff,
                            this.IsInternetSession,
                            this.IsPaused,
                            this.TotalPayment
                            ), sysTime, toKillProcesses);
                    toSinchronize = false;
                }
                catch (Exception e)
                {
                    System.Windows.MessageBox.Show(this.Name + ":\n" + "Method: SendUpdateMessageToClient()\n" + e.Message, "", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                }
        }

        public void ToEndSession()
        {
            if (isSessionStarted)
            {
                _Timer.Stop();
                _Timer = null;
                IsSessionStarted = false;
                NotifyPropertyChanged("Status");
                //RemainingTime = null;
                StartSessionTime = null;
                EndSessionTime = null;
                if (callback != null)
                    try
                    {
                        ((IClientContract)callback).SessionFinished();
                    }
                    catch (Exception e)
                    {
                        System.Windows.MessageBox.Show(this.Name + ":\n" + "Method: ToEndSession()" + "\n" + e.Message, "", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    }
            }
        }

        public void ToPauseSession()
        {
            if (isSessionStarted)
            {
                if (IsPaused) //already paused
                    this.IsPaused = false;
                else
                    this.IsPaused = true;
                this.SendUpdateMessageToClient(false);
            }
        }

        private void ToSinchronizeSession(object sessionMessage)
        {
            if (IsSessionStarted)
            {
                toSinchronize = true;
            }
            else if (isAlreadyWasConnected)
            {
                _Timer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(1) };
                _Timer.Tick += new EventHandler(delegate(object s, EventArgs a)
                {
                    SendUpdateMessageToClient(false);
                    _Timer.Stop();
                });
                _Timer.Start();
            }
            else
            {
                if (callback != null && sessionMessage != null)
                    try
                    {
                        ToCreate_Session(true, sessionMessage as SessionMessage, false);
                    }
                    catch (Exception e)
                    {
                        System.Windows.MessageBox.Show(this.Name + ":\n" + e.Message, "", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    }
            }
        }

        public void ToKillProcesses()
        {
            if (callback != null)
                try
                {
                    ((IClientContract)callback).KillProcesses();
                }
                catch (Exception e)
                {
                    System.Windows.MessageBox.Show(this.Name + ":\n" + e.Message, "", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                }
        }


        public void ToShutdown(string flag)
        {
            if (callback != null)
                try
                {
                    ((IClientContract)callback).Shutdown(flag);
                }
                catch (Exception e)
                {
                    System.Windows.MessageBox.Show(this.Name + ":\n" + e.Message, "", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                }
        }
        #endregion

        #region Properties
        public int Id
        {
            get { return id; }
            set
            {
                id = value;
                //NotifyPropertyChanged("Id");
            }
        }

        public Boolean IsUnlim
        {
            get { return !Tariff.LimitedTimeMode; }
        }

        public String Name
        {
            get { return name; }
            set
            {
                name = value;
                NotifyPropertyChanged("Name");
            }
        }

        public String IP
        {
            get { return ip; }
            set
            {
                ip = value;
                NotifyPropertyChanged("IP");
            }
        }

        public bool IsConnected
        {
            get { return callback != null ? ((ICommunicationObject)callback).State == CommunicationState.Opened : false; }
        }

        public bool IsSessionStarted
        {
            get { return isSessionStarted; }
            set
            {
                isSessionStarted = value;
                NotifyPropertyChanged("Status");
            }
        }

        public String RemainingTime
        {
            get
            {
                if (IsSessionStarted)
                {
                    if (IsPaused)
                    {
                        return "пауза";
                    }
                    else if (Tariff.LimitedTimeMode)
                    {
                        return (SessionExpire.Value - DateTime.Now).ToString(@"hh\:mm\:ss");
                    }
                }
                return "--";
            }
            set
            {
                if (value != null)
                {
                    if (!SessionStart.HasValue)
                    {
                        SessionStart = DateTime.Now;
                        NotifyPropertyChanged("StartSessionTime");
                    }
                    var remainingTime = Convert.ToDouble(value);
                    SessionExpire = SessionStart.Value.AddSeconds(remainingTime);
                    NotifyPropertyChanged("EndSessionTime");
                    NotifyPropertyChanged("RemainingTime");
                }
            }
        }

        public Double RemainingTimeInDouble
        {
            get { return SessionStart.HasValue && SessionExpire.HasValue ? (SessionExpire.Value - DateTime.Now).TotalSeconds : -1; }
        }

        public String StartSessionTime
        {
            get { return SessionStart.HasValue ? SessionStart.Value.ToString("HH:mm:ss") : "--"; }
            set
            {
                SessionStart = value != null ? DateTime.ParseExact(value, "HH:mm:ss", null) : (Nullable<DateTime>)null;
                NotifyPropertyChanged("StartSessionTime");
                NotifyPropertyChanged("RemainingTime");
            }
        }

        public Nullable<DateTime> StartSession
        {
            get { return SessionStart; }
            set
            {
                SessionStart = value;
                NotifyPropertyChanged("StartSessionTime");
                NotifyPropertyChanged("RemainingTime");
            }
        }

        public Nullable<DateTime> EndSession
        {
            get { return SessionExpire; }
            set
            {
                SessionExpire = value;
                NotifyPropertyChanged("EndSessionTime");
                NotifyPropertyChanged("RemainingTime");
            }
        }

        public String EndSessionTime
        {
            get { return IsSessionStarted && Tariff.LimitedTimeMode ? SessionExpire.Value.ToString("HH:mm:ss") : "--"; }
            set
            {
                SessionExpire = value != null ? DateTime.ParseExact(value, "HH:mm:ss", null) : (Nullable<DateTime>)null;
                NotifyPropertyChanged("EndSessionTime");
                NotifyPropertyChanged("RemainingTime");
            }
        }

        //Status:
        // connected and in session = 0 (_IsConnected == true && _isSessionStarted == true) - green
        // connected and not in session = 1 (_IsConnected == true && _isSessionStarted == false) - red
        // disconnected and in session = 2 (_IsConnected == false && _isSessionStarted == true) - yellow
        // disconnected and not in session = 3 (_IsConnected == false && _isSessionStarted == false) - black
        public int Status
        {
            get
            {

                return IsConnected ?
                    (isSessionStarted ? 0 : 1)
                    : (isSessionStarted ? 2 : 3);
            }

        }
        #endregion

        #region Callback
        public object Callback
        {
            get { return callback; }
        }
        public void Connected(object callback, object sessionMessage)
        {
            this.callback = callback;
            ToSinchronizeSession(sessionMessage);
            isAlreadyWasConnected = true;
            NotifyPropertyChanged("Status");
        }
        public void Disconnected()
        {
            callback = null;
            NotifyPropertyChanged("Status");
        }
        #endregion

        private void NotifyPropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}