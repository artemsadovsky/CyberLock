using System;
using System.ServiceModel;

namespace SunRise.CyberLock.Common.Library.IServiceContracts
{
    [ServiceContract(CallbackContract = typeof(IClientContract))]
    [ServiceKnownType(typeof(SunRise.CyberLock.Common.Library.Data.SessionMessage))]
    public interface IServerContract
    {
        [OperationContract]
        bool Subscribe(string machineName, Object sessionMessage);
        [OperationContract(IsOneWay = true)]
        void Unsubscribe();

    }
}
