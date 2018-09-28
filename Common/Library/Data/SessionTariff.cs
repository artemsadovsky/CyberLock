using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Xml.Linq;
using System.IO;

namespace SunRise.CyberLock.Common.Library.Data
{
    [Serializable]
    public class SessionTariff : AbstractTariff
    {
        public override string Name { get; set; }
        public override bool LimitedTimeMode { get; set; }
        public override Double CostPerHourGame { get; set; }
        public override Double CostPerHourInternet { get; set; }
    }

    public static class SessionTariffHelper
    {
        public static ObservableCollection<SessionTariff> LoadPriceFromXML(String fileName)
        {
            var collection = new ObservableCollection<SessionTariff>();

            if (File.Exists(fileName))
            {
                var pricesList = from price in XElement.Load(@fileName).Elements("price") select price;
                foreach (var pl in pricesList)
                {
                    SessionTariff price = new SessionTariff
                    {
                        Name = pl.Attribute("name").Value,
                        LimitedTimeMode = Boolean.Parse(pl.Attribute("mode").Value),
                        CostPerHourGame = uint.Parse(pl.Attribute("costPerHourGame").Value),
                        CostPerHourInternet = uint.Parse(pl.Attribute("costPerHourInternet").Value)
                    };
                    collection.Add(price);
                }
            }
            return collection;
        }

        public static bool SavePriceToXML(String fileName, ObservableCollection<SessionTariff> price)
        {
            XDocument doc = new XDocument(
                new XElement("prices",
                    from p in price
                    select new XElement("price",
                            new XAttribute("name", p.Name),
                            new XAttribute("mode", p.LimitedTimeMode),
                            new XAttribute("costPerHourGame", p.CostPerHourGame != null ? p.CostPerHourGame.ToString() : ""),
                            new XAttribute("costPerHourInternet", p.CostPerHourInternet != null ? p.CostPerHourInternet.ToString() : "")
                    )
                )
            );
            doc.Save(fileName);
            return true;
        }
    }
}
