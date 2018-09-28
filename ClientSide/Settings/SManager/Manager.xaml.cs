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
using SunRise.CyberLock.Common.Library.Helper;
using SunRise.CyberLock.ClientSide.Settings.SLib;

namespace SunRise.CyberLock.ClientSide.Settings.SManager
{
    /// <summary>
    /// Interaction logic for Manager.xaml
    /// </summary>
    public partial class Manager : Window
    {
        Configuration conf = new Configuration();

        public Manager()
        {
            InitializeComponent();
        }
    }
}
