using System;
using System.ServiceModel;

namespace SunRise.CyberLock.Common.Library.IServiceContracts
{
    [ServiceContract]
    [ServiceKnownType(typeof(SunRise.CyberLock.Common.Library.Data.SessionMessage))]
    [ServiceKnownType(typeof(SunRise.CyberLock.Common.Library.Helper.SYSTEMTIME))]
    public interface IClientContract
    {
        [OperationContract(IsOneWay = true)]
        void Ping();

        [OperationContract(IsOneWay = true)]
        void ServerDisconnect();

        [OperationContract]
        void SessionUpdated(Object session, SunRise.CyberLock.Common.Library.Helper.SYSTEMTIME sysTime, Boolean toKillProcesses);

        [OperationContract]
        void SessionFinished();

        [OperationContract]
        void KillProcesses();

        /// <summary>
        ///  Shutdown,restart and log off Computer
        /// </summary>
        /// <param name="Flags">Specify the Shut down Parameter
        ///  "1" - Shut down
        ///  "2" - Restart
        ///  "0" - Log off
        /// </param>
        [OperationContract]
        void Shutdown(String Flag);

        //[OperationContract]
        //void SessionTick(DateTime sessionExpire);
    }
}
