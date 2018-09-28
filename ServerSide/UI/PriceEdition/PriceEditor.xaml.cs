using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Xceed.Wpf.Toolkit;
using Xceed.Wpf.Toolkit.PropertyGrid;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;
using ItemCollection = Xceed.Wpf.Toolkit.PropertyGrid.Attributes.ItemCollection;
using System.Collections.ObjectModel;
using System.ComponentModel;
using SunRise.CyberLock.ServerSide.DAL;
using SunRise.CyberLock.ServerSide.DAL.Tariff;
using SunRise.CyberLock.ServerSide.BL.Helper;

namespace SunRise.CyberLock.ServerSide.UI.PriceEdition
{
    /// <summary>
    /// Interaction logic for PriceEditor.xaml
    /// </summary>
    public partial class PriceEditor : Window, INotifyPropertyChanged
    {
        public ObservableCollection<EditionTariff> price = new ObservableCollection<EditionTariff>();
        private EditionTariff selectedPrice;
        public EditionTariff SelectedPrice
        {
            get { return selectedPrice; }
            set
            {
                selectedPrice = value;
                RaisePropertyChanged("SelectedPrice");
            }
        }
        public PriceEditor()
        {
            InitializeComponent();
            price = EditionTariffHelper.LoadPriceFromXML(Constants.GetPricePath());
            listView_Price.ItemsSource = price;
            DataContext = this;
        }
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void RaisePropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            EditionTariffHelper.SavePriceToXML(Constants.GetPricePath(), price);
            this.Close();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (tbNewPrice.Text.Length > 0)
            {
                price.Add(new EditionTariff { Name = tbNewPrice.Text, LimitedTimeMode = false, CostPerHourGame = 0, CostPerHourInternet = 0 });
                tbNewPrice.Text = String.Empty;
            }
        }

        private void tbNewPrice_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (tbNewPrice.Text.Length > 0)
                btnAdd.IsEnabled = true;
            else btnAdd.IsEnabled = false;
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedPrice != null)
            {
                price.Remove(SelectedPrice);
            }
        }
    }
}
