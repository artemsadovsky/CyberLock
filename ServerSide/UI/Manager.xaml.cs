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
using System.Windows.Threading;
using System.ComponentModel;
using SunRise.CyberLock.ServerSide.BL;
using SunRise.CyberLock.ServerSide.DAL;
using SunRise.CyberLock.ServerSide.DAL.Tariff;
using SunRise.CyberLock.Common.Library.Data;
using SunRise.CyberLock.ServerSide.UI.PriceEdition;
using SunRise.CyberLock.ServerSide.UI.SessionEdition;

namespace SunRise.CyberLock.ServerSide.UI
{
    /// <summary>
    /// Interaction logic for Manager.xaml
    /// </summary>
    public partial class Manager : Window
    {
        public DateTime DateTimeNow { get; set; }

        public MainHandler mainHandler;

        public Manager()
        {
            InitializeComponent();
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, (DispatcherOperationCallback)delegate(object o)
            {
                mainHandler = new MainHandler();
                listView_clients.ItemsSource = mainHandler.GetClients();
                mainHandler.StartServer();
                return null;
            }, null);

            DispatcherTimer timer = new DispatcherTimer(new TimeSpan(0, 0, 1), DispatcherPriority.Normal, delegate
            {
                tb_Clock.Text = DateTime.Now.ToString("HH:mm:ss");
                ShowDetails();
            }, this.Dispatcher);
        }

        #region GetDataFromListBox(ListBox,Point)
        private static object GetDataFromListBox(ListBox source, Point point)
        {
            UIElement element = source.InputHitTest(point) as UIElement;
            if (element != null)
            {
                object data = DependencyProperty.UnsetValue;
                while (data == DependencyProperty.UnsetValue)
                {
                    data = source.ItemContainerGenerator.ItemFromContainer(element);

                    if (data == DependencyProperty.UnsetValue)
                    {
                        element = VisualTreeHelper.GetParent(element) as UIElement;
                    }

                    if (element == source)
                    {
                        return null;
                    }
                }

                if (data != DependencyProperty.UnsetValue)
                {
                    return data;
                }
            }

            return null;
        }
        #endregion

        #region ListView_clients events

        private Boolean toCleanAdditionalDetails = false;
        private void ShowDetails()
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, (DispatcherOperationCallback)delegate(object o)
            {
                if (listView_clients.SelectedItem != null && (listView_clients.SelectedItem as Client).IsSessionStarted)
                {
                    tb_tarrifName.Content = (listView_clients.SelectedItem as Client).Tariff.Name;
                    if ((listView_clients.SelectedItem as Client).Tariff.LimitedTimeMode && (listView_clients.SelectedItem as Client).SessionExpire.HasValue)
                    {
                        tb_tarrifCost.Content = (listView_clients.SelectedItem as Client).Tariff.CostPerHourGame.ToString();
                        tb_totalMoney.Content = (listView_clients.SelectedItem as Client).TotalPayment.ToString();
                        tb_workMoney.Content = Math.Round((listView_clients.SelectedItem as Client).TotalPayment - ((listView_clients.SelectedItem as Client).Tariff.CostPerHourGame * ((listView_clients.SelectedItem as Client).SessionExpire.Value - DateTime.Now).TotalHours), 0).ToString();
                    }
                    toCleanAdditionalDetails = true;
                }
                return null;
            }, null);
        }

        private void ShowDetailsClick(object sender, RoutedEventArgs e)
        {
            ShowDetails();
        }

        private void listView_clients_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            listView_clients.UnselectAll();
            //button_createSession.IsEnabled = false;
            //button_closeSession.IsEnabled = false;
            //button_changeSession.IsEnabled = false;
            //button_addTimeToSession.IsEnabled = false;
            //button_addPaymentToSession.IsEnabled = false;
        }

        private void listView_clients_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, (DispatcherOperationCallback)delegate(object o)
            {
                if (toCleanAdditionalDetails)
                {
                    tb_tarrifName.Content = "";
                    tb_tarrifCost.Content = "";
                    tb_totalMoney.Content = "";
                    tb_workMoney.Content = "";
                    toCleanAdditionalDetails = false;
                }
                return null;
            }, null);
            //if (Mouse.LeftButton != MouseButtonState.Pressed)
            //    return;
            //object data = GetDataFromListBox(this.listView_clients, e.GetPosition(this.listView_clients));

            //if (data != null)
            //{
            //    DragDrop.DoDragDrop(this.listView_clients, data, DragDropEffects.All);
            //}
            //button_createSession.IsEnabled = true;
            //button_closeSession.IsEnabled = true;
            //button_changeSession.IsEnabled = true;
            //button_addTimeToSession.IsEnabled = true;
            //button_addPaymentToSession.IsEnabled = true;
        }

        private void ListBox_Drop(object sender, DragEventArgs e)
        {
            ListBox parent = (ListBox)sender;
            object data = e.Data.GetData(typeof(Client));
            //((IList)this.listView_clients.ItemsSource).Remove(data);
            //parent.Items.Add(data);
        }

        //BindingListCollectionView blcv;
        GridViewColumnHeader lastHeaderClicked = null;
        //ListSortDirection _lastDirection = ListSortDirection.Ascending;
        SortingOrder lastOrder = SortingOrder.Ascending;

        private void listView_clients_Header_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader headerClicked =
            (GridViewColumnHeader)e.OriginalSource;
            //ListSortDirection direction;
            SortingOrder order = SortingOrder.Ascending;

            if (headerClicked != null)
            {
                if (headerClicked.Role != GridViewColumnHeaderRole.Padding)
                {
                    if (headerClicked != lastHeaderClicked)
                    {
                        order = SortingOrder.Ascending;
                    }
                    else
                    {
                        if (lastOrder == SortingOrder.Ascending)
                        {
                            order = SortingOrder.Descending;
                        }
                        else
                        {
                            order = SortingOrder.Ascending;
                        }
                    }

                    if (headerClicked.Column.Header is string)
                    {
                        string sortBy = headerClicked.Column.Header as string;
                        mainHandler.SortClients(sortBy, order);
                    }
                    else if (headerClicked.Column.Header is Object)
                    {
                        Object header = headerClicked.Column.Header as Object;
                        string type = header.ToString();
                        mainHandler.SortClientsByHeaderType(type, order);
                    }
                    else
                    {
                        return;
                    }

                    listView_clients.ItemsSource = mainHandler.GetClients();
                    //Sort(sortBy, direction);

                    lastHeaderClicked = headerClicked;
                    lastOrder = order;
                }
            }
        }

        private void Sort(string sortBy, ListSortDirection direction)
        {
            
            ICollectionView dataView =
              CollectionViewSource.GetDefaultView(listView_clients.ItemsSource);

            dataView.SortDescriptions.Clear();
            SortDescription sd = new SortDescription(sortBy, direction);
            dataView.SortDescriptions.Add(sd);
            dataView.Refresh();
        }
        #endregion

        #region Events of session buttons
        private void CreateSession(object sender, RoutedEventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, (DispatcherOperationCallback)delegate(object o)
            {
                if (listView_clients.SelectedItems.Count > 0)
                {
                    bool toCreate = true;
                    foreach (Client client in listView_clients.SelectedItems)
                    {
                        if (client.IsSessionStarted)
                        {
                            toCreate = MessageBox.Show("Будет создан новый сеанс. Продолжить?", "", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes ? true : false;
                            break;
                        }
                    }
                    if (toCreate)
                    {
                        var wnd = new SessionCreationView { Owner = this };
                        wnd.View(listView_clients.SelectedItems);
                    }
                }
                else
                {
                    MessageBox.Show("Не выбран ни один клиент", "", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                if (sender is Button)
                {
                    var scope = FocusManager.GetFocusScope(sender as Button); // elem is the UIElement to unfocus
                    FocusManager.SetFocusedElement(scope, null); // remove logical focus
                    Keyboard.ClearFocus(); // remove keyboard focus
                }
                return null;
            }, null);
        }

        private void button_changeSession_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, (DispatcherOperationCallback)delegate(object o)
            {
                if (listView_clients.SelectedItems.Count > 0)
                {
                    bool isDifferent = false;
                    bool isNotValidToEdit = false;
                    bool isFirst = true;
                    bool isInternetSession = false;
                    DateTime expireDate = DateTime.Now;
                    SessionTariff tarrif = new SessionTariff();
                    Double payment = 0;
                    foreach (Client client in listView_clients.SelectedItems)
                    {
                        if (client.EndSession.HasValue && client.Tariff.LimitedTimeMode)
                        {
                            if (isFirst)
                            {
                                payment = client.TotalPayment;
                                expireDate = client.EndSession.Value;
                                tarrif = client.Tariff;
                                isInternetSession = client.IsInternetSession;
                                isFirst = false;
                            }
                            else
                            {
                                if (!isDifferent)
                                    if (!(
                                        expireDate < client.EndSession.Value.AddSeconds(5)
                                        && expireDate > client.EndSession.Value.AddSeconds(-5)
                                        && tarrif.Equals(client.Tariff)
                                        && isInternetSession.Equals(client.IsInternetSession)
                                        ))
                                    {
                                        isDifferent = true;
                                    }
                            }
                        }
                        else
                        {
                            isNotValidToEdit = true;
                            break;
                        }
                    }
                    if (isNotValidToEdit)
                    {
                        if (MessageBox.Show("Будет создан новый сеанс. Продолжить?", "", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                        {
                            var wnd = new SessionCreationView { Owner = this };
                            wnd.View(listView_clients.SelectedItems);
                        }
                    }
                    else
                    {
                        if (!isDifferent) //сеансы одинаковые
                        {
                            var wnd = new SessionExtensionView { Owner = this };
                            wnd.View(listView_clients.SelectedItems, expireDate, tarrif, payment);
                        }
                        else
                        {
                            MessageBox.Show("Сеансы различны и не могут быть изменены!\nПопробуйте добавить платеж или время.", "", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Не выбран ни один клиент", "", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                var scope = FocusManager.GetFocusScope(sender as Button); // elem is the UIElement to unfocus
                FocusManager.SetFocusedElement(scope, null); // remove logical focus
                Keyboard.ClearFocus(); // remove keyboard focus       
                return null;
            }, null);
        }

        private void button_addPaymentToSession_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, (DispatcherOperationCallback)delegate(object o)
            {
                if (listView_clients.SelectedItems.Count > 0)
                {
                    bool isDifferent = false;
                    bool isNotValidToEdit = false;
                    bool isFirst = true;
                    bool isInternetSession = false;
                    SessionTariff tarrif = new SessionTariff();
                    Double payment = 0;
                    foreach (Client client in listView_clients.SelectedItems)
                    {
                        if (client.EndSession.HasValue && client.Tariff.LimitedTimeMode)
                        {
                            if (isFirst)
                            {
                                payment = client.TotalPayment;
                                tarrif = client.Tariff;
                                isInternetSession = client.IsInternetSession;
                                isFirst = false;
                            }
                            else
                            {
                                if (!isDifferent)
                                    if (!(
                                        tarrif.Equals(client.Tariff)
                                        && isInternetSession.Equals(client.IsInternetSession)
                                        ))
                                    {
                                        isDifferent = true;
                                    }
                            }
                        }
                        else
                        {
                            isNotValidToEdit = true;
                            break;
                        }
                    }
                    if (isNotValidToEdit)
                    {
                        if (MessageBox.Show("Будет создан новый сеанс. Продолжить?", "", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                        {
                            var wnd = new SessionCreationView { Owner = this };
                            wnd.View(listView_clients.SelectedItems);
                        }
                    }
                    else
                    {
                        var wnd = new PaymentEditionView { Owner = this };
                        if (isDifferent)
                            wnd.View(listView_clients.SelectedItems);
                        else
                            wnd.View(listView_clients.SelectedItems, tarrif, isInternetSession);
                    }
                }
                else
                {
                    MessageBox.Show("Не выбран ни один клиент", "", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                var scope = FocusManager.GetFocusScope(sender as Button); // elem is the UIElement to unfocus
                FocusManager.SetFocusedElement(scope, null); // remove logical focus
                Keyboard.ClearFocus(); // remove keyboard focus
                return null;
            }, null);
        }

        private void button_addTimeToSession_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, (DispatcherOperationCallback)delegate(object o)
            {
                if (listView_clients.SelectedItems.Count > 0)
                {
                    bool isNotValidToEdit = false;
                    SessionTariff tarrif = new SessionTariff();
                    foreach (Client client in listView_clients.SelectedItems)
                    {
                        if (!(client.EndSession.HasValue && client.Tariff.LimitedTimeMode))
                        {
                            isNotValidToEdit = true;
                            break;
                        }
                    }
                    if (isNotValidToEdit)
                    {
                        if (MessageBox.Show("Будет создан новый сеанс. Продолжить?", "", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                        {
                            var wnd = new SessionCreationView { Owner = this };
                            wnd.View(listView_clients.SelectedItems);
                        }
                    }
                    else
                    {
                        var wnd = new TimeEditionView { Owner = this };
                        wnd.View(listView_clients.SelectedItems);
                    }
                }
                else
                {
                    MessageBox.Show("Не выбран ни один клиент", "", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                var scope = FocusManager.GetFocusScope(sender as Button); // elem is the UIElement to unfocus
                FocusManager.SetFocusedElement(scope, null); // remove logical focus
                Keyboard.ClearFocus(); // remove keyboard focus
                return null;
            }, null);
        }


        private void button_closeSession_Click(object sender, RoutedEventArgs e)
        {
            if (listView_clients.SelectedItems.Count > 0)
            {
                if (MessageBox.Show("Завершить сеанс?", "", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes ? true : false)
                    foreach (Client client in listView_clients.SelectedItems)
                    {
                        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, (DispatcherOperationCallback)delegate(object o)
                        {
                            client.ToEndSession();
                            return null;
                        }, null);
                    }
            }
            else
            {
                MessageBox.Show("Не выбран ни один клиент", "", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            //mainHandler.CloseSession(listView_clients.SelectedItems);
            var scope = FocusManager.GetFocusScope(sender as Button); // elem is the UIElement to unfocus
            FocusManager.SetFocusedElement(scope, null); // remove logical focus
            Keyboard.ClearFocus(); // remove keyboard focus
        }


        private void button_pauseSession_Click(object sender, RoutedEventArgs e)
        {
            if (listView_clients.SelectedItems.Count > 0)
            {
                foreach (Client client in listView_clients.SelectedItems)
                {
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, (DispatcherOperationCallback)delegate(object o)
                    {
                        client.ToPauseSession();
                        return null;
                    }, null);
                }
            }
            else
            {
                MessageBox.Show("Не выбран ни один клиент", "", MessageBoxButton.OK, MessageBoxImage.Information);
            }
                //mainHandler.PauseSession(listView_clients.SelectedItems);
                var scope = FocusManager.GetFocusScope(sender as Button); // elem is the UIElement to unfocus
                FocusManager.SetFocusedElement(scope, null); // remove logical focus
                Keyboard.ClearFocus(); // remove keyboard focus
        }

        private void button_KillProcesses_Click(object sender, RoutedEventArgs e)
        {
            if (listView_clients.SelectedItems.Count > 0)
            {
                foreach (Client client in listView_clients.SelectedItems)
                {
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, (DispatcherOperationCallback)delegate(object o)
                    {
                        client.ToKillProcesses();
                        return null;
                    }, null);
                }
            }
            else
            {
                MessageBox.Show("Не выбран ни один клиент", "", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            var scope = FocusManager.GetFocusScope(sender as Button); // elem is the UIElement to unfocus
            FocusManager.SetFocusedElement(scope, null); // remove logical focus
            Keyboard.ClearFocus(); // remove keyboard focus
        }

        private void button_Shutdown_Click(object sender, RoutedEventArgs e)
        {
            if (listView_clients.SelectedItems.Count > 0)
            {
                foreach (Client client in listView_clients.SelectedItems)
                {
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, (DispatcherOperationCallback)delegate(object o)
                    {
                        client.ToShutdown("1");
                        return null;
                    }, null);
                }
            }
            else
            {
                MessageBox.Show("Не выбран ни один клиент", "", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            var scope = FocusManager.GetFocusScope(sender as Button); // elem is the UIElement to unfocus
            FocusManager.SetFocusedElement(scope, null); // remove logical focus
            Keyboard.ClearFocus(); // remove keyboard focus
        }

        private void button_Restart_Click(object sender, RoutedEventArgs e)
        {
            if (listView_clients.SelectedItems.Count > 0)
            {
                foreach (Client client in listView_clients.SelectedItems)
                {
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, (DispatcherOperationCallback)delegate(object o)
                    {
                        client.ToShutdown("2");
                        return null;
                    }, null);
                }
            }
            else
            {
                MessageBox.Show("Не выбран ни один клиент", "", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            var scope = FocusManager.GetFocusScope(sender as Button); // elem is the UIElement to unfocus
            FocusManager.SetFocusedElement(scope, null); // remove logical focus
            Keyboard.ClearFocus(); // remove keyboard focus
        }
        #endregion

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, (DispatcherOperationCallback)delegate(object o)
            {
                mainHandler.StopServer();
                return null;
            }, null);
        }

        private void menu_PriceEdit_Click(object sender, RoutedEventArgs e)
        {
            var wnd = new PriceEditor { Owner = this };
            wnd.ShowDialog();
        }

        private void listView_clients_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ShowDetails();
        }
    }
}
