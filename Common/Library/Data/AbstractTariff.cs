using System;

namespace SunRise.CyberLock.Common.Library.Data
{
    [Serializable]
    public abstract class AbstractTariff
    {
        public abstract string Name { get; set; }
        public abstract bool LimitedTimeMode { get; set; }
        public abstract Double CostPerHourGame { get; set; }
        public abstract Double CostPerHourInternet { get; set; }
    }
}
