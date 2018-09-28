using System;
using SunRise.CyberLock.Common.Library.Data;

namespace SunRise.CyberLock.ClientSide.DAL
{
    [Serializable]
    public class Session : AbstractSession
    {
        public Boolean IsActiveSession
        {
            get { return SessionExpire.HasValue ? SessionExpire.Value > DateTime.Now : false; }
        }

        public virtual String RemainingTime
        {
            get
            {
                if (Tariff == null || !SessionExpire.HasValue)
                {
                    return "Время истекло";
                }
                else if (Tariff.LimitedTimeMode)
                {
                    return (SessionExpire.Value - DateTime.Now).ToString(@"hh\:mm\:ss");
                }
                else
                {
                    return "Безлимитный режим";
                }
            }
            set { }
        }

        public String LastFiveMin()
        {
            if (SessionExpire.HasValue)
            {
                return (SessionExpire.Value - DateTime.Now).ToString(@"mm\:ss");
            }
            return "";
        }

        public void UpdateSession(SessionMessage session)
        {
            this.SessionStart = session.SessionStart;
            this.SessionExpire = session.SessionExpire;
            this.Tariff = session.Tariff;
            this.TotalPayment = session.TotalPayment;
            this.IsInternetSession = session.IsInternetSession;
            this.IsPaused = session.IsPaused;
        }

        public void Finished()
        {
            this.SessionStart = null;
            this.SessionExpire = null;
            this.Tariff = null;
            this.TotalPayment = 0;
            this.IsInternetSession = false;
            this.IsPaused = false;
        }
    }
}