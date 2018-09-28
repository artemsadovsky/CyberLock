using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SunRise.CyberLock.Common.Library.Data
{
    [Serializable]
    public class SessionMessage : AbstractSession
    {
        public SessionMessage() { }
        public SessionMessage(Nullable<DateTime> sessionStart, Nullable<DateTime> sessionExpire, SessionTariff tariff, Boolean isInternetSession, Double totalPayment)
        {
            this.SessionStart = sessionStart;
            this.SessionExpire = sessionExpire;
            this.Tariff = tariff;
            this.IsInternetSession = isInternetSession;
            this.TotalPayment = totalPayment;
        }
        public SessionMessage(Nullable<DateTime> sessionStart, Nullable<DateTime> sessionExpire, SessionTariff tariff, Boolean isInternetSession, Boolean isPaused, Double totalPayment)
        {
            this.SessionStart = sessionStart;
            this.SessionExpire = sessionExpire;
            this.Tariff = tariff;
            this.IsInternetSession = isInternetSession;
            this.IsPaused = isPaused;
            this.TotalPayment = totalPayment;
        }
    }
}
