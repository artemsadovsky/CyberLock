using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.ComponentModel;
using Xceed.Wpf.Toolkit;
using Xceed.Wpf.Toolkit.PropertyGrid;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;
using ItemCollection = Xceed.Wpf.Toolkit.PropertyGrid.Attributes.ItemCollection;
using System.Collections.ObjectModel;
using System.Windows.Controls.Primitives;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.Windows.Data;

namespace SunRise.CyberLock.ServerSide.DAL.Tariff
{
    [Serializable]
    [DisplayName("Тариф")]
    public class EditionTariff : SunRise.CyberLock.Common.Library.Data.AbstractTariff
    {
        [Category("Информация")]
        [DisplayName("Название тарифа")]
        [Editor(typeof(NameEditor), typeof(NameEditor))]
        public override string Name { get; set; }

        [Category("Режим работы")]
        [DisplayName("Режим времени")]
        [ItemsSource(typeof(TimeModeItemSource))]
        public override bool LimitedTimeMode { get; set; }

        [Category("Режим работы")]
        [DisplayName("Стоимость игры")]
        [Editor(typeof(CostPerHourEditor), typeof(CostPerHourEditor))]
        public override Double CostPerHourGame { get; set; }

        [Category("Режим работы")]
        [DisplayName("Стоимость интернета")]
        [Editor(typeof(CostPerHourEditor), typeof(CostPerHourEditor))]
        public override Double CostPerHourInternet { get;set; }

        public bool Equals(EditionTariff obj)
        {
            if (CostPerHourGame == obj.CostPerHourGame
                && CostPerHourInternet == obj.CostPerHourInternet
                //&& Name == (obj as Tariff).Name
                && LimitedTimeMode == obj.LimitedTimeMode)
                return true;
            else return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }


    public class NameEditor : ITypeEditor
    {

        public FrameworkElement ResolveEditor(PropertyItem propertyItem)
        {
            TextBox textBox = new TextBox();

            EditionTariff source = propertyItem.Instance as EditionTariff;
            Debug.Assert(source != null);

            var textBinding = new Binding("Value")
            {
                Source = propertyItem,
                ValidatesOnExceptions = true,
                ValidatesOnDataErrors = true,
                Mode = propertyItem.IsReadOnly ? BindingMode.OneWay : BindingMode.TwoWay
            };

            BindingOperations.SetBinding(textBox, TextBox.TextProperty, textBinding);

            return textBox;
        }
    }

    public class TimeModeItemSource : IItemsSource
    {
        public ItemCollection GetValues()
        {
            ItemCollection modes = new ItemCollection();

            modes.Add(false, "Без ограничения");
            modes.Add(true, "Ограничение по времени");

            return modes;
        }
    }

    public class CostPerHourEditor : ITypeEditor
    {

        public FrameworkElement ResolveEditor(PropertyItem propertyItem)
        {
            TextBox textBox = new TextBox();

            EditionTariff source = propertyItem.Instance as EditionTariff;
            Debug.Assert(source != null);

            var isEnabledBinding = new Binding("LimitedTimeMode")
            {
                Source = propertyItem.Instance,
                ValidatesOnExceptions = true,
                ValidatesOnDataErrors = true,
                Mode = propertyItem.IsReadOnly ? BindingMode.OneWay : BindingMode.TwoWay,
                TargetNullValue = false
            };

            var textBinding = new Binding("Value")
            {
                Source = propertyItem,
                ValidatesOnExceptions = true,
                ValidatesOnDataErrors = true,
                Mode = propertyItem.IsReadOnly ? BindingMode.OneWay : BindingMode.TwoWay
            };

            BindingOperations.SetBinding(textBox, TextBox.IsEnabledProperty, isEnabledBinding);
            BindingOperations.SetBinding(textBox, TextBox.TextProperty, textBinding);

            return textBox;
        }
    }

}
