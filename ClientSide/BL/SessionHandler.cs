using System;
using System.IO;
using SunRise.CyberLock.ClientSide.DAL;
using SunRise.CyberLock.Common.Library.Helper;
using SunRise.CyberLock.Common.Library.Data;
using SunRise.CyberLock.ClientSide.Settings.SLib;

namespace SunRise.CyberLock.ClientSide.BL
{
    public class SessionHandler
    {
        private readonly Session _session = new Session();
        private readonly object _syncWriteLock = new object();

        public SessionHandler()
        {
            Locker.AppLogger.Debug("SessionHandler()", "Enter");
            try
            {
                var obj = Serializer.LoadFromXML<Session>(Constants.GetSessionPath());
                if (obj != null)
                    mapSession((Session)obj);
                //obj = null;
            }
            catch (Exception ex)
            {
                Locker.AppLogger.Error("SessionHandler()", ex.Message);
            }
            Locker.AppLogger.Debug("SessionHandler()", "Finish");
        }

        private void mapSession(Session newSession)
        {
            if (newSession != null)
            {
                _session.IsInternetSession = newSession.IsInternetSession;
                _session.IsPaused = newSession.IsPaused;
                _session.SessionExpire = newSession.SessionExpire;
                _session.SessionStart = newSession.SessionStart;
                _session.Tariff = newSession.Tariff;
                _session.TotalPayment = newSession.TotalPayment;
            }
        }

        public Session ClientSession
        {
            get { return _session; }
        }

        public SessionMessage SessionMessage
        {
            get
            {
                return new SessionMessage(
                             _session.SessionStart,
                             _session.SessionExpire,
                             _session.Tariff,
                             _session.IsInternetSession,
                             _session.IsPaused,
                             _session.TotalPayment);
            }
        }

        public Boolean IsActiveSession
        {
            get { return _session.IsActiveSession; }
        }

        public Boolean IsPaused
        {
            get { return _session.IsPaused; }
        }

        public void UpdateBySessionMessage(object sessionMessage)
        {
            ClientSession.UpdateSession(sessionMessage as SessionMessage);
            if (this.IsActiveSession)
            {
                lock (_syncWriteLock)
                {
                    ClientSession.SaveToXml(Constants.CONFIG_DATA_PATH, Constants.CONFIG_SESSION_FILE_NAME);
                }
            }
            else
            {
                SessionFinished();
            }
        }

        public void SessionFinished()
        {
            ClientSession.Finished();
            if (File.Exists(Constants.GetSessionPath())) {
                lock (_syncWriteLock)
                {
                    File.Delete(Constants.GetSessionPath());
                }
            }
        }

        public String LastFiveMin()
        {
            return ClientSession.LastFiveMin();
        }

        public Boolean LessThenFiveMin() {
            return (
                ClientSession.SessionExpire.HasValue
                && ClientSession.SessionStart.HasValue
                && (ClientSession.SessionExpire.Value - DateTime.Now).TotalMinutes < 5
                );
        }
    }
}
