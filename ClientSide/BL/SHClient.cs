using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading;
using System.Windows.Threading;
using SunRise.CyberLock.Common.Library.IServiceContracts;
using SunRise.CyberLock.Common.Library.Helper;
using SunRise.CyberLock.Common.Library.Logger;
using SunRise.CyberLock.ClientSide.DAL;
using SunRise.CyberLock.ClientSide.Settings.SLib;

namespace SunRise.CyberLock.ClientSide.BL
{
    public class SHClient : IClientContract, IDisposable
    {

        public readonly SessionHandler _sessionHandler = new SessionHandler();
        
        private readonly AppLogger HostLogger = new AppLogger(Locker.AppSettings.FIELDS.Log_Level, Constants.CONFIG_LOG_PATH, Constants.HOST_LOG_NAME);

        static object locker = new object();
        private InstanceContext context;
        private Binding binding;
        private EndpointAddress adress;

        private IServerContract proxy;


        //private Timer reconnectTimer;
        Thread reconnectThread;

        private DispatcherTimer workTimer;
        private int unrespondedTime = 0;

        #region Delegates
        public delegate void EventOpened();
        public delegate void EventFaulted();
        public delegate void EventClosed();
        public delegate void EventSimple();
        public delegate void EventDebug(String method, String action);
        public delegate void EventUpdateSession(object session, bool toKillProcesses);
        public delegate void EventShutdown(String Flag);
        #endregion

        #region Events
        public event EventOpened EOpened;
        public event EventFaulted EFaulted;
        public event EventClosed EClosed;
        public event EventUpdateSession ESessionUpdated;
        public event EventSimple ESessionFinished;
        public event EventShutdown EShutdown;
        public event EventSimple EKillProcesses;
        #endregion

        private void WriteErrorLog(String method, String message)
        {
            HostLogger.Error(method, message);
        }

        private void WriteDebugLog(String method, String message)
        {
            HostLogger.Debug(method, message);
        }

        public void DoWork()
        {
            context = new InstanceContext(this);
            binding = new NetTcpBinding(SecurityMode.None) { ReceiveTimeout = new TimeSpan(0, 0, 20) };

            //adress = new EndpointAddress("net.tcp://192.168.2.168:1234/ClubControl");
            //adress = new EndpointAddress("net.tcp://192.168.2.159:1234/ClubControl");
            //adress = new EndpointAddress("net.tcp://localhost:1234/ClubControl");
            adress = new EndpointAddress(Locker.AppSettings.GetAddress());

            ToReconnect();
            InitWorkerTime();
        }

        private void InitWorkerTime()
        {
            if (workTimer == null)
            {
                workTimer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(1) };
                workTimer.Tick += new EventHandler(delegate(object s, EventArgs a)
                {
                    unrespondedTime++;
                    if (proxy != null && unrespondedTime > 7)
                    {
                        try
                        {
                            unrespondedTime = 0;
                            proxy.Unsubscribe();
                            AbortProxy(true);
                        }
                        catch (CommunicationException ce)
                        {
                            WriteErrorLog("InitWorkerTime()", "Exception: " + ce.Message);
                        }
                        catch (ObjectDisposedException ode)
                        {
                            WriteErrorLog("InitWorkerTime()", "Exception: " + ode.Message);
                        }
                    }
                });
            }
        }

        private void ReconnectHandler()
        {
            WriteDebugLog("ReconnectHandler()", "Enter");
            while (!TryConnect())
            {
                WriteDebugLog("ReconnectHandler()", "Not connected");
                Thread.Sleep(5000);
                //dispatcher.Invoke(new Action(() => connected = TryConnect()));
            }
        }

        private Boolean TryConnect()
        {
            lock (locker)
            {
                proxy = (new DuplexChannelFactory<IServerContract>(context, binding, adress)).CreateChannel();

                ((ICommunicationObject)proxy).Opened += new EventHandler(
                    delegate { EOpened(); });
                ((ICommunicationObject)proxy).Faulted += (sender, args) => Proxy_Faulted(sender, args);
                ((ICommunicationObject)proxy).Faulted += new EventHandler(
                    delegate { EFaulted(); });


                try
                {
                    proxy.Subscribe(
                        Environment.MachineName,
                        _sessionHandler.IsActiveSession ?
                            _sessionHandler.SessionMessage
                            : null);

                    unrespondedTime = 0;
                    workTimer.Start();
                    return true;
                }
                catch (CommunicationException ce)
                {
                    WriteErrorLog("TryConnect()", "Ping Exception: " + ce.Message);
                    return false;
                }
            }

        }

        private void ToReconnect()
        {
            if (reconnectThread == null || !reconnectThread.IsAlive)
            {
                reconnectThread = new Thread(new ThreadStart(ReconnectHandler));
                reconnectThread.Start();
            }
        }

        private void AbortProxy(Boolean toDoReconnect)
        {
            if (proxy != null)
            {
                ((ICommunicationObject)proxy).Abort();
                ((ICommunicationObject)proxy).Close();
                proxy = null;
            }
            if (toDoReconnect)
            {
                ToReconnect();
            }
            else
            {
                if (reconnectThread != null)
                {
                    reconnectThread.Abort();
                }
            }
        }

        private void Proxy_Faulted(object sender, EventArgs e)
        {
            //EFaulted();
            workTimer.Stop();

            IChannel channel = sender as IChannel;
            if (channel != null)
            {
                channel.Abort();
                channel.Close();
            }

            AbortProxy(true);
        }


        #region IClientContract methods
        public void Ping()
        {
            unrespondedTime = 0;
        }

        public void ServerDisconnect()
        {
            EClosed();
            AbortProxy(true);
        }

        public void SessionUpdated(Object session, SYSTEMTIME sysTime, Boolean toKillProcesses)
        {
            SystemTime.SetSystemTime(ref sysTime);
            ESessionUpdated(session, toKillProcesses);
        }

        public void SessionFinished() { ESessionFinished(); }

        public Object SinchronizeSession()
        {
            if (_sessionHandler.IsActiveSession)
            {
                return _sessionHandler.SessionMessage;
            }
            else return null;
        }

        public void KillProcesses()
        {
            EKillProcesses();
        }

        public void Shutdown(String Flag)
        {
            EShutdown(Flag);
        }
        #endregion

        #region IDisposable method
        public void Dispose()
        {
            if (proxy != null)
            {
                if (((ICommunicationObject)proxy).State == CommunicationState.Opened)
                    proxy.Unsubscribe();
            }
            if (reconnectThread != null)
            {
                reconnectThread.Abort();
            }
            AbortProxy(false);
            if (workTimer != null)
                workTimer.Stop();
        }
        #endregion
    }
}
