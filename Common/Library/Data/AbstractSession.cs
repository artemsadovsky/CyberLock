using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SunRise.CyberLock.Common.Library.Data
{
    [Serializable]
    public abstract class AbstractSession
    {
        public Nullable<DateTime> SessionStart;
        public Nullable<DateTime> sessionExpire;
        public SessionTariff Tariff { get; set; }
        public bool IsInternetSession = false;
        public bool IsPaused = false;
        //Оплаченная сумма. Поле для истории, не связанное с остальными полями
        public Double TotalPayment = 0;

        public Nullable<DateTime> SessionExpire
        {
            get
            {
                if (Tariff == null)
                {
                    return null;
                }
                else
                {
                    return Tariff.LimitedTimeMode ? sessionExpire : DateTime.Now.AddHours(23);
                }
            }
            set { sessionExpire = value; }
        }

        public AbstractSession() { }
    }
}
