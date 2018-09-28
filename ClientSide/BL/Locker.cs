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
using System.Threading;
using SunRise.CyberLock.Common.Library.Logger;
using SunRise.CyberLock.ClientSide.Settings.SLib;
using SunRise.CyberLock.ClientSide.DAL;
using SunRise.CyberLock.ClientSide.Utils.Graphics;

namespace SunRise.CyberLock.ClientSide.BL
{
    public class Locker: Window
    {
        public static readonly Configuration AppSettings = new Configuration();
        private static AppLogger _AppLogger;
        private static readonly object _lockLoggerObj = new object();
        public static AppLogger AppLogger
        {
            get
            {
                lock (_lockLoggerObj)
                {
                    if (_AppLogger == null)
                    {
                        _AppLogger = new AppLogger(AppSettings.FIELDS.Log_Level, Constants.CONFIG_LOG_PATH, Constants.FULL_LOG_NAME);
                    }
                    return _AppLogger;
                }
            }
        }

        private readonly SHClient _shClient = new SHClient();
        public TaskbarNotifierHandler _taskbarNotifierHandler { get; set; }
        
        #region System functions  
        #region unused
        /*[return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern bool BlockInput([In, MarshalAs(UnmanagedType.Bool)] bool fBlockIt);
        
        [DllImport("user32.dll", SetLastError = true)]
        static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);*/
        #endregion
        //[DllImport("user32.dll", SetLastError = true)]
        //static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
                
        //[DllImport("User32.dll")]
        //[return: MarshalAs(UnmanagedType.Bool)]
        //private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern int ShowCursor(bool bShow);

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hwnd, int cmd);

        //[DllImport("user32.dll", SetLastError = true)]
        //private static extern IntPtr GetDC(IntPtr hWnd);

        //[DllImport("user32.dll")]
        //private static extern IntPtr AttachThreadInput(IntPtr idAttach, IntPtr idAttachTo, int fAttach);

        //[DllImport("user32.dll")]
        //private static extern bool AllowSetForegroundWindow(int procID);
        #endregion
        
        #region Keybord hooks
        private bool IsRequestingOptions_Unlock { get { return (Keyboard.Modifiers == (ModifierKeys.Alt | ModifierKeys.Control) && Keyboard.IsKeyDown(Key.U)); } }
        private bool IsRequestingOptions_Exit { get { return (Keyboard.Modifiers == (ModifierKeys.Alt | ModifierKeys.Control) && Keyboard.IsKeyDown(Key.Y)); } }
        private bool IsCtrlAltDelete { get { return (Keyboard.Modifiers == (ModifierKeys.Alt | ModifierKeys.Control) && Keyboard.IsKeyDown(Key.Delete)); } }
        #endregion
        
        #region Password
        private String EnteredPassword = "";
        private readonly String pass = "ghjktnfhbfn";
        private Boolean PasswordModeToExit = false;
        private Boolean PasswordModeToUnlock = false;
        #endregion

        #region UI components
        protected Label BigInformer;
        protected Label SmallInformer;
        protected Label ErrorInformer;
        protected Label PasswordModeInformer;
        protected Label MiddleInformer;
        protected Grid GridFinisher;
        #endregion

        protected bool isLocked = false;
        private bool isError = false;

        protected Boolean reallyCloseWindow = false;
         
        private int seconds_ToTheEnd = AppSettings.FIELDS.KP_TimeWaiting;

        private enum STATES
        {
            Session,
            NotSession,
            LastFiveMinutes,
            Pause,
            FinishingSession,
            AdminSession
        };
        private STATES state = STATES.NotSession;

        #region ExceptionEvent (not used)
        /*// CLR unhandled exception
        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            AppLogger.Error("OnUnhandledException()", "Exception: " + e.ExceptionObject);
            return;
        }

        // Windows Forms unhandled exception
        private static void OnGuiUnhandedException(object sender, ThreadExceptionEventArgs e)
        {
            AppLogger.Error("OnGuiUnhandedException()", "Exception: " + e.Exception);
            return;
        }*/
        #endregion

        #region Init methods

        private void InitializeLocker()
        {
            //AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(OnUnhandledException);

            try
            {
                this.Hide();
                InitializeSHClient();
                this.Lock();
                this.InitializeTimers();
                workingTimer.Start();
                _taskbarNotifierHandler = new TaskbarNotifierHandler(_shClient._sessionHandler.ClientSession);
                RunInspector();
                AppLogger.Debug("InitializeLocker()", "All components initialized successfully.");
            }
            catch (Exception ex)
            {
                this.isError = true;
                LockSystem();
                this.ErrorInformer.Content = "ERROR: " + ex.Message;
                AppLogger.Error("InitializeLocker()", ex.Message);
            }

        }

        private void InitializeSHClient()
        {
            
            _shClient.EOpened += new SHClient.EventOpened(ConnectionOpened);
            _shClient.EFaulted += new SHClient.EventFaulted(ConnectionFaulted);
            _shClient.EClosed += new SHClient.EventClosed(ConnectionClosed);
            _shClient.ESessionUpdated += new SHClient.EventUpdateSession(SessionUpdated);
            _shClient.ESessionFinished += new SHClient.EventSimple(SessionFinished);
            _shClient.EShutdown += new SHClient.EventShutdown(ToShutdown);
            _shClient.EKillProcesses += new SHClient.EventSimple(ToKillProcesses);
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, (DispatcherOperationCallback)delegate(object o)
            {
                try
                {
                    this._shClient.DoWork();
                }
                catch (Exception ex)
                {
                    LockSystem();
                    this.ErrorInformer.Content = "ERROR: " + ex.Message;
                    AppLogger.Error("InitializeSHClient()", ex.Message);
                }
                return null;
            }, null);
        }

        private void InitializeTimers()
        {
            InitWorkingTimer();
            InitRemainingTimer();
        }

        private void RunInspector()
        {
            try
            {

                if (Process.GetProcessesByName(Constants.INSPECTOR_APP_NAME).Length == 0)
                {
                    Process pr = new Process();
                    pr.StartInfo.FileName = AppSettings.FIELDS.App_Folder + Constants.INSPECTOR_FILE_NAME;
                    pr.Start();
                }
            }
            catch (Exception ex)
            {
                AppLogger.Error("RunInspector()", ex.Message);
            }
        }

        #endregion

        #region WorkingTimer
        private DispatcherTimer workingTimer;
        private void InitWorkingTimer()
        {
            if (workingTimer == null)
            {
                workingTimer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(1) };
                workingTimer.Tick += new EventHandler(WorkingTimerTick);
            }
        }

        private void WorkingTimerTick(object obj, EventArgs args)
        {
            switch (state)
            {
                case STATES.FinishingSession:
                    {
                        if (seconds_ToTheEnd > 0)
                        {
                            this.GridFinisher.Visibility = Visibility.Visible;
                            MiddleInformer.Content = String.Format("{0} секунд(-ы).", seconds_ToTheEnd);
                            seconds_ToTheEnd--;
                            MinimizeAllWindows();
                            if (!ProcessManager.IsForegrount())
                            {
                                this.Topmost = false;
                                this.Topmost = true;
                                InvalidateVisual();
                            }
                        }
                        else
                            State = STATES.NotSession;
                        break;
                    }
                case STATES.NotSession:
                    {
                        if (this.IsActiveSession)
                            State = STATES.Session;
                        else
                        {
                            if (!isLocked)
                                Lock();
                            if (!ProcessManager.IsForegrount())
                            {
                                this.Topmost = false;
                                this.Topmost = true;
                                InvalidateVisual();
                            }
                        }
                        
                        break;
                    }
                case STATES.Session:
                case STATES.LastFiveMinutes:
                    {
                        if (!this.IsActiveSession)
                            State = STATES.FinishingSession;
                        else if (this.IsPaused)
                            State = STATES.Pause;
                        else if (
                            state != STATES.LastFiveMinutes
                            && SessionHandler.LessThenFiveMin())
                        {
                            State = STATES.LastFiveMinutes;
                        }
                        break;
                    }
                case STATES.Pause:
                    {
                        if (!this.IsActiveSession)
                            State = STATES.FinishingSession;
                        else if (!this.IsPaused)
                            State = STATES.Session;
                        break;
                    }
                case STATES.AdminSession:
                    {
                        if (this.IsActiveSession)
                            State = STATES.Session;
                        break;
                    }
            }
        }
        #endregion

        #region RemainingTimer
        private DispatcherTimer remainingTimer;
        private readonly object _remSyncLock = new object();
        private readonly DrawHandler _drawHandler = new DrawHandler(Locker.AppSettings.FIELDS.RemTimer_Width, Locker.AppSettings.FIELDS.RemTimer_Height, Locker.AppSettings.FIELDS.RemTimer_FontSize);

        private void InitRemainingTimer()
        {
            AppLogger.Debug("InitRemainingTimer()", DateTime.Now.ToString("mm:ss:ff"));
        }

        private void RemainingTimerTick(object s, EventArgs a)
        {
            //AppLogger.Debug("RemainingTimerTick()", "Date: " + DateTime.Now.ToString("mm:ss:ff"));
            try
            {
                _drawHandler.Draw(SessionHandler.LastFiveMin());
            }
            catch (Exception ex)
            {
                AppLogger.Error("TimerTick()", ex.Message);
            }
            finally
            {
                //AppLogger.Debug("RemainingTimerTick()", "State is " + State + ". Date: " + DateTime.Now.ToString("mm:ss:ff"));
                if(State != STATES.LastFiveMinutes)
                    StopRemainingTimer();
            }
        }

        private void CleanTimerScreen()
        {
            AppLogger.Debug("CleanTimerScreen()", "Date: " + DateTime.Now.ToString("mm:ss:ff"));
            try
            {
                _drawHandler.CleanScreen();
            }
            catch (Exception ex)
            {
                AppLogger.Error("CleanTimerScreen()", ex.Message);
            }
        }

        private void StartRemainingTimer()
        {
            Locker.AppLogger.Debug("StartRemainingTimer()", "Enter - " + DateTime.Now.ToString("mm:ss:ff"));
            lock (_remSyncLock)
            {
                if (remainingTimer == null)
                {
                    remainingTimer = new DispatcherTimer();
                    remainingTimer.Interval = TimeSpan.FromMilliseconds(1);
                    remainingTimer.Tick += new EventHandler(RemainingTimerTick);
                }
                if (!remainingTimer.IsEnabled)
                    remainingTimer.Start();
            }

            Locker.AppLogger.Debug("StartRemainingTimer()", "Finish - " + DateTime.Now.ToString("mm:ss:ff"));
        }

        private void StopRemainingTimer()
        {
            Locker.AppLogger.Debug("StopRemainingTimer()", DateTime.Now.ToString("mm:ss:ff"));
            lock (_remSyncLock)
            {
                if (remainingTimer != null && remainingTimer.IsEnabled)
                {
                    remainingTimer.Stop();
                    CleanTimerScreen();
                }
            }
        }
        #endregion

        private STATES State
        {
            get { return state; }
            set
            {
                STATES prevState = state;
                state = value;
                switch (state)
                {
                    case STATES.LastFiveMinutes:
                        {
                            StartRemainingTimer();
                            break;
                        }
                    case STATES.Pause:
                        {
                            //StopRemainingTimer();
                            Pause();
                            break;
                        }
                    case STATES.FinishingSession:
                        {
                            //StopRemainingTimer();
                            Lock();
                            seconds_ToTheEnd = AppSettings.FIELDS.KP_TimeWaiting;
                            break;
                        }
                    case STATES.NotSession:
                        {
                            //StopRemainingTimer();
                            ToKillProcesses();
                            Lock();
                            break;
                        }
                    case STATES.Session:
                        {
                            //StopRemainingTimer();
                            if(isLocked)
                                Unlock();
                            break;
                        }
                    case STATES.AdminSession:
                        {
                            Unlock();
                            break;
                        }
                }
            }
        }              

        #region Lock/Unlock/Pause

        private void LockSystem()
        {
            KeyboardManager.DisableSystemKeys();
            isLocked = true;
            this.Show();
            ShowCursor(false);
        }

        private void Lock()
        {
            CleanLockerScreen();
            this.BigInformer.Content = "Сеанс завершен";
            LockSystem();
        }

        private void Unlock()
        {
            if (!isError)
            {
                isLocked = false;

                ShowCursor(true);
                this.Hide();
                this.ShowInTaskbar = false;
                KeyboardManager.EnableSystemKeys();
            }
        }

        private void Pause()
        {
            CleanLockerScreen();
            this.BigInformer.Content = "Пауза";
            LockSystem();
        }


        private void CleanLockerScreen()
        {
            if (!isError)
            {
                this.GridFinisher.Visibility = Visibility.Hidden;
                this.MiddleInformer.Content = "";
                this.SmallInformer.Content = "";
                this.BigInformer.Content = "";
                this.ErrorInformer.Content = "";

                PasswordModeToExit = false;
                PasswordModeToUnlock = false;
                this.PasswordModeInformer.Visibility = Visibility.Hidden;
            }
        }
        #endregion

        
        private void ToClose()
        {
            Unlock();
            this.Close();
        }

        #region Properties
        protected SessionHandler SessionHandler
        {
            get { return _shClient._sessionHandler; }
        }
                
        protected Boolean IsActiveSession
        {
            get { return SessionHandler.ClientSession.IsActiveSession; }
            set { }
        }

        public Boolean IsPaused
        {
            get { return SessionHandler.ClientSession.IsPaused; }
            set { }
        }
        #endregion

        #region SHClient Events
        private void SessionUpdated(object session, Boolean toKillProcesses)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, (DispatcherOperationCallback)delegate(object o)
                {
                    try
                    {
                        AppLogger.Debug("SessionUpdated()", "Enter");
                        if (toKillProcesses)
                            ToKillProcesses();
                        SessionHandler.UpdateBySessionMessage(session);
                        if (IsActiveSession)
                            State = STATES.Session;
                        else if(State != STATES.NotSession)
                            State = STATES.FinishingSession;
                        AppLogger.Debug("SessionUpdated()", "Finish");
                    }
                    catch (Exception ex)
                    {
                        AppLogger.Error("SessionUpdated()", "Error: " + ex.Message + ". State is " + State);
                    }
                    return null;
                }, null);
        }

        private void SessionFinished()
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, (DispatcherOperationCallback)delegate(object o)
            {
                try
                {
                    SessionHandler.SessionFinished();
                    State = STATES.FinishingSession;
                }
                catch (Exception ex)
                {
                    AppLogger.Error("SessionFinished(): " + ex.Message);
                }
                return null;
            }, null);
        }

        private void ConnectionOpened()
        {
            AppLogger.Debug("ConnectionOpened()", "Connection OPENED");
        }
        private void ConnectionFaulted()
        {
            AppLogger.Debug("ConnectionFaulted()", "Connection FAULTED");
        }
        private void ConnectionClosed()
        {
            AppLogger.Debug("ConnectionClosed()", "Connection CLOSED");
        }

        private void ToShutdown(String flag)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, (DispatcherOperationCallback)delegate(object o)
            {
                ProcessManager.ShutDownWindows(flag);         
                return null;
            }, null);
        }

        private void ToKillProcesses()
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, (DispatcherOperationCallback)delegate(object o)
            {
                ProcessManager.KillAllProcesses(AppSettings.FIELDS.KP_IncludesList, AppSettings.FIELDS.KP_ExlusionsList, true);
                return null;
            }, null);
        }

        private void MinimizeAllWindows()
        {
            System.Diagnostics.Process[] etc = System.Diagnostics.Process.GetProcesses();//получаем процессы
            foreach (System.Diagnostics.Process anti in etc)//перебираем
            {
                try
                {
                    if (anti.MainWindowTitle.ToString() != "") //отлавливаем процессы, которые имеют окна
                    {
                        if (!anti.MainWindowTitle.ToString().Equals("Locker"))
                        {
                            ShowWindow(anti.MainWindowHandle, 6); //сворачивам окна
                        }
                    }
                }
                catch (Exception ex)
                {
                    AppLogger.Error("MinimizeAllWindows()", ex.Message);
                }
            }
        }
        #endregion

        #region Window Events

        private bool _inStateChange;

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == WindowState.Maximized && !_inStateChange)
            {
                _inStateChange = true;
                WindowState = WindowState.Normal;
                WindowStyle = WindowStyle.None;
                WindowState = WindowState.Maximized;
                ResizeMode = ResizeMode.NoResize;
                _inStateChange = false;
            }
            base.OnStateChanged(e);
        }

        protected void Locker_Closing()
        {            
                if (workingTimer != null && workingTimer.IsEnabled) workingTimer.Stop();
                //if (remainingTimer != null && remainingTimer.IsEnabled) remainingTimer.Stop();
                StopRemainingTimer();

                if (_shClient != null) _shClient.Dispose();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            try
            {
                if (isLocked)
                {
                    if (IsRequestingOptions_Exit)
                    {
                        EnteredPassword = "";
                        PasswordModeToExit = true;
                        PasswordModeToUnlock = false;
                        this.PasswordModeInformer.Visibility = Visibility.Visible;
                        this.PasswordModeInformer.Content = "Password Mode (TO EXIT)";
                        return;
                    }
                    else
                        if (IsRequestingOptions_Unlock)
                        {
                            EnteredPassword = "";
                            PasswordModeToExit = false;
                            PasswordModeToUnlock = true;
                            this.PasswordModeInformer.Visibility = Visibility.Visible;
                            this.PasswordModeInformer.Content = "Password Mode (TO UNLOCK)";
                            return;
                        }
                        else
                            if (IsCtrlAltDelete)
                            {
                                MessageBox.Show("Ctrl+Alt+Delete");
                                e.Handled = true;
                                return;
                            }
                            else
                                if (e.Key != Key.System &&
                                    e.Key != Key.LeftAlt && e.Key != Key.RightAlt &&
                                    e.Key != Key.LeftCtrl && e.Key != Key.RightCtrl)
                                {
                                    if ((PasswordModeToExit || PasswordModeToUnlock) && e.Key == Key.Enter)
                                    {
                                        if (EnteredPassword.Equals(pass, StringComparison.InvariantCultureIgnoreCase))
                                        {
                                            if (PasswordModeToUnlock)
                                            {
                                                State = STATES.AdminSession;
                                            }
                                            else
                                            {
                                                ProcessManager.KillProcessByName(Constants.INSPECTOR_APP_NAME);
                                                this.reallyCloseWindow = true;
                                                this.Close();
                                            }
                                        }
                                        else
                                        {
                                            EnteredPassword = "";
                                            PasswordModeToExit = false;
                                            PasswordModeToUnlock = false;
                                            this.PasswordModeInformer.Visibility = Visibility.Hidden;
                                        }
                                        return;
                                    }
                                    else
                                    {
                                        if ((PasswordModeToExit || PasswordModeToUnlock))
                                        {
                                            EnteredPassword += e.Key.ToString();
                                            return;
                                        }
                                        else if (state == STATES.FinishingSession && e.Key == Key.R)
                                        {
                                            seconds_ToTheEnd = AppSettings.FIELDS.KP_TimeWaiting;
                                        }
                                    }
                                }
                }

            }
            catch (Exception ex)
            {
                AppLogger.Error("OnKeyDown()", "key = '" + e.Key + "' Exception: " + ex);
            }
            finally
            {
                base.OnKeyDown(e);
            }
        }

        protected void Locker_Loaded()
        {
            InitializeLocker();
        }
        #endregion


    }
    
}
