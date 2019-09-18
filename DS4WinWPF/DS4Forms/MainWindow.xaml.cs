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
using Microsoft.Win32;
//using Xceed.Wpf.Toolkit;

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
            //logListView.ItemsSource = logvm.LogItems;
            logListView.DataContext = logvm;
            lastMsgLb.DataContext = lastLogMsg;
            
            profilesListBox.ItemsSource = profileListHolder.ProfileListCol;

            Task.Delay(5000).ContinueWith((t) =>
            {
                //logvm.LogItems.Add(new LogItem { Datetime = DateTime.Now, Message = "Next Thing" });
                profileListHolder.ProfileListCol.Add(new ProfileEntity { Name = "Media" });
                AppLogger.LogToGui("Next Thing", true);
            });

            App root = Application.Current as App;
            StartStopBtn.Content = root.rootHub.Running ? "Stop" : "Start";

            conLvViewModel = new ControllerListViewModel(root.rootHub, profileListHolder);
            controllerLV.DataContext = conLvViewModel;
            ChangeControllerPanel();
            trayIconVM = new TrayIconViewModel(root.rootHub);
            notifyIcon.DataContext = trayIconVM;
            PopulateSettingsTab();
            SetupEvents();

            Task.Run(() =>
            {
                root.rootHub.Start();
            });
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
                MessageBox.Show(temp.Message, "Log");
            }
        }

        private void ClearLogBtn_Click(object sender, RoutedEventArgs e)
        {
            logvm.LogItems.Clear();
        }

        private void MainTabCon_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (mainTabCon.SelectedIndex == 4)
            {
                lastMsgLb.Visibility = Visibility.Hidden;
            }
            else
            {
                lastMsgLb.Visibility = Visibility.Visible;
            }
        }

        private void ProfilesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            newProfBtn.IsEnabled = true;
            editProfBtn.IsEnabled = true;
            deleteProfBtn.IsEnabled = true;
            dupProfBtn.IsEnabled = true;
            importProfBtn.IsEnabled = true;
            exportProfBtn.IsEnabled = true;
        }

        private void PopulateSettingsTab()
        {
            hideDS4ContCk.IsChecked = true;
        }

        private void RunAtStartCk_Click(object sender, RoutedEventArgs e)
        {
            runAsGroupBox.Visibility = runAtStartCk.IsChecked == true ? Visibility.Visible :
                Visibility.Collapsed;
        }

        private void ContStatusImg_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            CompositeDeviceModel item = conLvViewModel.CurrentItem;
            DS4Device tempDev = item.Device;
            if (tempDev.Synced && !tempDev.Charging)
            {
                if (tempDev.ConnectionType == ConnectionType.BT)
                {
                    tempDev.StopUpdate();
                    tempDev.DisconnectBT();
                }
                else if (tempDev.ConnectionType == ConnectionType.SONYWA)
                {
                    tempDev.DisconnectDongle();
                }
            }
        }

        private void ExportLogBtn_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.AddExtension = true;
            dialog.DefaultExt = ".txt";
            dialog.Filter = "Text Documents (*.txt)|*.txt";
            dialog.Title = "Select Export File";
            // TODO: Expose config dir
            //dialog.InitialDirectory = Global.appdatapath;
            if (dialog.ShowDialog() == true)
            {
                LogWriter logWriter = new LogWriter(dialog.FileName, logvm.LogItems.ToList());
                logWriter.Process();
            }
        }

        private void IdColumnTxtB_ToolTipOpening(object sender, ToolTipEventArgs e)
        {
            TextBlock statusBk = sender as TextBlock;
            int idx = Convert.ToInt32(statusBk.Tag);
            if (idx >= 0)
            {
                CompositeDeviceModel item = conLvViewModel.ControllerCol[idx];
                item.RequestUpdatedTooltipID();
            }
        }

        /// <summary>
        /// Clear and re-populate tray context menu
        /// </summary>
        private void NotifyIcon_TrayRightMouseUp(object sender, RoutedEventArgs e)
        {
            trayIconVM.BuildContextMenu(notifyIcon.ContextMenu.Items, this);
        }

        /// <summary>
        /// Change profile based on selection
        /// </summary>
        private void SelectProfCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox box = sender as ComboBox;
            int idx = Convert.ToInt32(box.Tag);
            if (idx > -1)
            {
                //CompositeDeviceModel item = conLvViewModel.ControllerCol[idx];
            }
        }

        private void CustomColorPick_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {

        }

        private void LightColorBtn_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            button.ContextMenu.IsOpen = true;
        }
    }
}
