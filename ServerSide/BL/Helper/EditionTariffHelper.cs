using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Xml.Linq;
using System.IO;
using SunRise.CyberLock.ServerSide.DAL.Tariff;

namespace SunRise.CyberLock.ServerSide.BL.Helper
{
    public static class EditionTariffHelper
    {
        public static ObservableCollection<EditionTariff> LoadPriceFromXML(String fileName)
        {
            var collection = new ObservableCollection<EditionTariff>();

            if (File.Exists(fileName))
            {
                var pricesList = from price in XElement.Load(@fileName).Elements("price") select price;
                foreach (var pl in pricesList)
                {
                    EditionTariff price = new EditionTariff
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

        public static bool SavePriceToXML(String fileName, ObservableCollection<EditionTariff> price)
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
