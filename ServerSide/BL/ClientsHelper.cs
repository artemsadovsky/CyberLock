using System;
using System.Linq;
using System.Xml.Linq;
using System.Collections.ObjectModel;
using SunRise.CyberLock.ServerSide.DAL;
using System.IO;

namespace SunRise.CyberLock.ServerSide.BL
{
    public static class ClientsHelper
    {
        public static bool ClientConnected(this ObservableCollection<Client> clientsCollection, object callback, string name, string host, Object sessionMessage)
        {
            if (clientsCollection.Where(c => c.Callback != null).Count(c => c.Callback.Equals(callback)) == 0)
            {
                var client = clientsCollection.FirstOrDefault(c => c.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
                if (client != null)
                {
                    if (client.Callback == null)
                    {
                        client.Connected(callback, sessionMessage);
                        return true;
                    }
                    else
                    {
                        int i = 1;
                        var changedName = name + " (" + i + ")";
                        while (clientsCollection.Count(c => c.Name.Equals(changedName)) != 0)
                        {
                            i++;
                            changedName = name + " (" + i + ")";
                        }
                        name = changedName;
                    }
                }

                Client newClient = new Client()
                {
                    Name = name,
                    IP = host
                };
                newClient.Connected(callback, sessionMessage);
                clientsCollection.Add(newClient);
                return true;
            }
            return false;
        }

        public static void ClientDisconnected(this ObservableCollection<Client> clientsCollection, object callback)
        {
            Client client = clientsCollection.Where(c => c.Callback != null).FirstOrDefault(c => c.Callback.Equals(callback));
            if (client != null)
            {
                client.Disconnected();
            }
        }

        public static ObservableCollection<Client> LoadClientsListFromXML(String fileName)
        {
            var collection = new ObservableCollection<Client>();

            if (File.Exists(fileName))
            {
                var computers = from comps in XElement.Load(@fileName).Elements("computer") select comps;
                foreach (var cl in computers)
                {
                    Client client = new Client
                    {
                        Id = int.Parse(cl.Attribute("id").Value),
                        Name = cl.Attribute("name").Value,
                        IP = cl.Attribute("ip").Value,
                        RemainingTime = null
                    };
                    collection.Add(client);
                }
            }
            return collection;
        }

        public static bool SaveClientsList(String fileName, ObservableCollection<Client> clients)
        {
            XDocument doc = new XDocument(
                new XElement("computers",
                    from c in clients
                    select new XElement("computer",
                            new XAttribute("id", c.Id),
                            new XAttribute("name", c.Name),
                            new XAttribute("ip", c.IP)
                    )
                )
            );
            doc.Save(fileName);
            return true;
        }

    }
}
