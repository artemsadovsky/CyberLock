using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ServiceModel;
using SunRise.CyberLock.Common.Library.IServiceContracts;
using System.ServiceModel.Channels;
using System.Windows.Threading;
using SunRise.CyberLock.ServerSide.DAL;

namespace SunRise.CyberLock.ServerSide.BL
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class SHServer : IServerContract
    {
        protected Boolean working = false;

        protected static ObservableCollection<Client> clientsCollection;
        private ServiceHost serviceHost = null;
        protected string serviceAddress;
        protected string endpointAddress;
        private Binding binding = new NetTcpBinding(SecurityMode.None);

        private System.Timers.Timer pingTimer;

        #region Initialization methods
        private void InitAndStartTimers()
        {
            pingTimer = new System.Timers.Timer(5000);
            pingTimer.Elapsed += new System.Timers.ElapsedEventHandler(PingTimerEvent);
            pingTimer.Start();
        }
        #endregion

        // Start Server
        protected void StartHostServer()
        {
            if (!working)
            {
                working = true;
                InitAndStartTimers();
                OpenHost();
            }
        }

        //Stop Server
        protected void StopHostServer()
        {
            Broadcast(BroadcastType.Disconnect);
            CloseHost();
            working = false;
        }

        private void PingTimerEvent(object sender, System.Timers.ElapsedEventArgs args)
        {
            Broadcast(BroadcastType.Ping);
        }

        #region Open/Stop server host
        private void OpenHost()
        {
            // Start the host
            serviceHost = new ServiceHost(
                 typeof(SHServer),
                 new Uri(serviceAddress));

            serviceHost.AddServiceEndpoint(
                typeof(IServerContract),
                binding,
                endpointAddress);

            serviceHost.Open();
        }

        private void CloseHost()
        {
            serviceHost.Close();
        }
        #endregion

        private void Broadcast(BroadcastType type)
        {
            List<IClientContract> list = (from item in clientsCollection
                                          where item.Callback != null
                                          select item.Callback as IClientContract).ToList();

            list.ForEach(delegate(IClientContract callback)
            {
                CommunicationState state = ((ICommunicationObject)callback).State;
                if (state == CommunicationState.Opened)
                {
                    try
                    {
                        switch (type)
                        {
                            case BroadcastType.Disconnect:
                                callback.ServerDisconnect();
                                break;
                            case BroadcastType.Ping:
                                callback.Ping();
                                break;
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
            });
        }

        private void Proxy_FaultedEvent(object sender, EventArgs args)
        {
            IClientContract callback = sender as IClientContract;
            clientsCollection.ClientDisconnected(callback);
        }

        #region IServerContract methods
        public bool Subscribe(string machineName, Object sessionMessage)
        {
            try
            {
                //Get the hashCode of the connecting app and store it as a connection
                OperationContext context = OperationContext.Current;
                MessageProperties prop = context.IncomingMessageProperties;

                RemoteEndpointMessageProperty endpointProperty = prop[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;

                IClientContract callback = context.GetCallbackChannel<IClientContract>();
                ((ICommunicationObject)callback).Faulted += new EventHandler(Proxy_FaultedEvent);
                if (clientsCollection.ClientConnected(callback, machineName, endpointProperty.Address + ":" + endpointProperty.Port, sessionMessage))
                {

                }
                else
                {
                    try
                    {
                        callback.ServerDisconnect();
                        ((ICommunicationObject)callback).Abort();
                        ((ICommunicationObject)callback).Close();
                    }
                    catch
                    {
                        Console.WriteLine();
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        public void Unsubscribe()
        {
            IClientContract callback = OperationContext.Current.GetCallbackChannel<IClientContract>();
            clientsCollection.ClientDisconnected(callback);
            try
            {
                ((ICommunicationObject)callback).Abort();
                ((ICommunicationObject)callback).Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        #endregion
    }
}
