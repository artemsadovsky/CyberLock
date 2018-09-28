using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using SunRise.CyberLock.ServerSide.DAL;
using SunRise.CyberLock.Common.Library.Helper;

namespace SunRise.CyberLock.ServerSide.BL
{
    public enum SortingOrder
    {
        Ascending = 0,
        Descending = 1
    }

    public class MainHandler : SHServer
    {
        Settings settings = new Settings();

        private void InitConnectionParams()
        {
            String hostName = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().HostName;
            serviceAddress = "net.tcp://" + hostName + ":" + settings.ServicePort;
            endpointAddress = settings.EndpointName;
        }

        public MainHandler()
        {
            var obj = Serializer.LoadFromXML<Settings>(Constants.GetSettingsPath());
            if (obj != null)
                settings = (Settings)obj;
            else
                settings.SaveToXml(Constants.CONFIG_DIR_PATH, Constants.CONFIG_SETTINGS_FILE_NAME);
            InitConnectionParams();
            clientsCollection = ClientsHelper.LoadClientsListFromXML(Constants.GetClientsPath());
        }

        public void StartServer()
        {
            StartHostServer();
        }

        public void StopServer()
        {
            base.StopHostServer();
        }

        #region Window actions
        //public void CloseSession(IList selectedClients)
        //{
        //    if (selectedClients.Count > 0
        //        && (MessageBox.Show("Завершить сеанс?", "", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes ? true : false))
        //        foreach (Client client in selectedClients)
        //        {
        //            client.ToEndSession();
        //        }
        //}

        //public void PauseSession(IList selectedClients)
        //{
        //    if (selectedClients.Count > 0)
        //        foreach (Client client in selectedClients)
        //        {
        //            client.ToPauseSession();
        //        }
        //}
        #endregion

        public ObservableCollection<Client> GetClients()
        {
            return clientsCollection;
        }

        /// <summary>
        /// code:
        ///     0 - sort by IsSessionStarted
        ///     1 - sort by Name
        ///     2 - sort by RemainingTime
        ///     3 - sort by StartSessionTime
        ///     4 - sort by EndSessionTime
        /// </summary>
        /// <param name="code"></param>
        /// <param name="order"></param>
        public void SortClients(string sortBy, SortingOrder order)
        {
            switch (sortBy)
            {
                case "":
                    {
                        clientsCollection = new ObservableCollection<Client>(
                            order == SortingOrder.Descending
                            ? clientsCollection.OrderByDescending(c => c.IsSessionStarted)
                            : clientsCollection.OrderBy(c => c.IsSessionStarted));
                        break;
                    }
                case "Осталось":
                    {
                        clientsCollection = new ObservableCollection<Client>(
                            order == SortingOrder.Ascending
                            ? clientsCollection.OrderByDescending(c => c.RemainingTimeInDouble).OrderByDescending(c => c.IsSessionStarted)
                            : clientsCollection.OrderBy(c => c.RemainingTimeInDouble).OrderByDescending(c => c.IsSessionStarted));
                        break;
                    }
                case "Старт":
                    {
                        clientsCollection = new ObservableCollection<Client>(
                            order == SortingOrder.Descending
                            ? clientsCollection.OrderByDescending(c => c.StartSession).OrderByDescending(c => c.IsSessionStarted)
                            : clientsCollection.OrderBy(c => c.StartSession).OrderByDescending(c => c.IsSessionStarted));
                        break;
                    }
                case "Стоп":
                    {
                        clientsCollection = new ObservableCollection<Client>(
                            order == SortingOrder.Descending
                            ? clientsCollection.OrderByDescending(c => c.EndSessionTime).OrderByDescending(c => c.IsSessionStarted)
                            : clientsCollection.OrderBy(c => c.EndSessionTime).OrderByDescending(c => c.IsSessionStarted));
                        break;
                    }
                case "Компьютер":
                default:
                    {
                        clientsCollection = new ObservableCollection<Client>(
                            order == SortingOrder.Descending
                            ? clientsCollection.OrderByDescending(c => c.Name)
                            : clientsCollection.OrderBy(c => c.Name));
                        break;
                    }
            }
            //clientsCollection.Order

        }

        public void SortClientsByHeaderType(string type, SortingOrder order)
        {
            if (type != null)
                if (type.Contains("Компьютер"))
                    SortClients("Компьютер", order);
                else if (type.Contains("Осталось"))
                    SortClients("Осталось", order);
                else if (type.Contains("Старт"))
                    SortClients("Старт", order);
                else if (type.Contains("Стоп"))
                    SortClients("Стоп", order);
                else SortClients("", order);
        }
    }
}
