using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DS4WinWPF.DS4Forms.ViewModel;
using DS4WinWPF;
using DS4Windows;

namespace DS4WinWPF.DS4Forms
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private StatusLogMsg lastLogMsg = new StatusLogMsg();
        private ProfileList profileListHolder = new ProfileList();
        private LogViewModel logvm;
        private ControllerListViewModel conLvViewModel;
        private TrayIconViewModel trayIconVM;

        public MainWindow()
        {
            InitializeComponent();

            logvm = new LogViewModel();
            logvm.LogItems.Add(new LogItem() { Datetime = DateTime.Now, Message = "Bacon" });
            //logListView.ItemsSource = logvm.LogItems;
            logListView.DataContext = logvm;
            lastMsgLb.DataContext = lastLogMsg;
            
            profilesListBox.ItemsSource = profileListHolder.ProfileListCol;

            Task.Delay(5000).ContinueWith((t) =>
            {
                //logvm.LogItems.Add(new LogItem { Datetime = DateTime.Now, Message = "Next Thing" });
                profileListHolder.ProfileListCol.Add(new ProfileEntity { Name = "Media" });

                //Dispatcher.BeginInvoke((Action)(() =>
                //{
                //lastLogMsg.Message = "Controller 1 Using \"dfsdfsdfs\"";
                AppLogger.LogToGui("Next Thing", true);
                //AppLogger.LogToTray("Test");

                //}));
            });

            App root = Application.Current as App;
            StartStopBtn.Content = root.rootHub.Running ? "Stop" : "Start";

            conLvViewModel = new ControllerListViewModel(root.rootHub, profileListHolder);
            controllerLV.DataContext = conLvViewModel;
            ChangeControllerPanel();
            trayIconVM = new TrayIconViewModel(root.rootHub);
            notifyIcon.DataContext = trayIconVM;
            SetupEvents();
        }

        private void ShowNotification(object sender, DebugEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                notifyIcon.ShowBalloonTip(TrayIconViewModel.ballonTitle,
                    e.Data, !e.Warning ? Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Info :
                    Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Warning);
            }));
        }

        private void SetupEvents()
        {
            App root = Application.Current as App;
            root.rootHub.RunningChanged += ControlServiceChanged;
            conLvViewModel.ControllerCol.CollectionChanged += ControllerCol_CollectionChanged;
            AppLogger.TrayIconLog += ShowNotification;
            AppLogger.GuiLog += UpdateLastStatusMessage;
        }

        private void UpdateLastStatusMessage(object sender, DebugEventArgs e)
        {
            lastLogMsg.Message = e.Data;
            lastLogMsg.Warning = e.Warning;
        }

        private void ChangeControllerPanel()
        {
            if (conLvViewModel.ControllerCol.Count == 0)
            {
                controllerLV.Visibility = Visibility.Hidden;
                noContLb.Visibility = Visibility.Visible;
            }
            else
            {
                controllerLV.Visibility = Visibility.Visible;
                noContLb.Visibility = Visibility.Hidden;
            }
        }

        private void ControllerCol_CollectionChanged(object sender,
            System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                ChangeControllerPanel();
            }));
        }

        private void ControlServiceChanged(object sender, EventArgs e)
        {
            Tester service = sender as Tester;
            Dispatcher.BeginInvoke((Action)(() =>
            {
                if (service.Running)
                {
                    StartStopBtn.Content = "Stop";
                }
                else
                {
                    StartStopBtn.Content = "Start";
                }
            }));
        }

        private void AboutBtn_Click(object sender, RoutedEventArgs e)
        {
            About aboutWin = new About();
            aboutWin.Owner = this;
            aboutWin.ShowDialog();
        }

        private async void StartStopBtn_Click(object sender, RoutedEventArgs e)
        {
            StartStopBtn.IsEnabled = false;
            App root = Application.Current as App;
            Tester service = root.rootHub;
            await Task.Run(() =>
            {
                if (service.Running)
                    service.Stop();
                else
                    service.Start();
            });

            StartStopBtn.IsEnabled = true;
        }

        private void LogListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            int idx = logListView.SelectedIndex;
            if (idx > -1)
            {
                LogItem temp = logvm.LogItems[idx];
                MessageBox.Show(temp.Message);
            }
        }

        private void ClearLogBtn_Click(object sender, RoutedEventArgs e)
        {
            logvm.LogItems.Clear();
        }
    }
}
