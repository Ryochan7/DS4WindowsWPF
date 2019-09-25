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
using System.Windows.Interop;
using System.Diagnostics;

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
        private SettingsViewModel settingsWrap;

        public MainWindow()
        {
            InitializeComponent();

            App root = Application.Current as App;
            settingsWrap = new SettingsViewModel();
            settingsTab.DataContext = settingsWrap;
            logvm = new LogViewModel(App.rootHub);
            //logListView.ItemsSource = logvm.LogItems;
            logListView.DataContext = logvm;
            lastMsgLb.DataContext = lastLogMsg;

            profileListHolder.Refresh();
            profilesListBox.ItemsSource = profileListHolder.ProfileListCol;

            StartStopBtn.Content = root.rootHubtest.Running ? "Stop" : "Start";

            conLvViewModel = new ControllerListViewModel(App.rootHub, profileListHolder);
            controllerLV.DataContext = conLvViewModel;
            ChangeControllerPanel();
            //trayIconVM = new TrayIconViewModel(root.rootHubtest);
            trayIconVM = new TrayIconViewModel(App.rootHub);
            notifyIcon.DataContext = trayIconVM;
            notifyIcon.Icon = Global.UseWhiteIcon ? Properties.Resources.DS4W___White :
                Properties.Resources.DS4W;

            SetupEvents();

            Task.Run(() =>
            {
                CheckDrivers();
                App.rootHub.Start();
                //root.rootHubtest.Start();
            });
        }

        private void ShowNotification(object sender, DS4Windows.DebugEventArgs e)
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
            App.rootHub.RunningChanged += ControlServiceChanged;
            //root.rootHubtest.RunningChanged += ControlServiceChanged;
            conLvViewModel.ControllerCol.CollectionChanged += ControllerCol_CollectionChanged;
            DS4Windows.AppLogger.TrayIconLog += ShowNotification;
            DS4Windows.AppLogger.GuiLog += UpdateLastStatusMessage;
            App.rootHub.Debug += UpdateLastStatusMessage;
        }

        private void UpdateLastStatusMessage(object sender, DS4Windows.DebugEventArgs e)
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
                foreach (CompositeDeviceModel item in e.NewItems)
                {
                    item.LightContext = new ContextMenu();
                    item.AddLightContextItems();
                    //item.LightContext.Items.Add(new MenuItem() { Header = "Use Profile Color", IsChecked = !item.UseCustomColor });
                    //item.LightContext.Items.Add(new MenuItem() { Header = "Use Custom Color", IsChecked = item.UseCustomColor });
                }
            }));
        }

        private void ControlServiceChanged(object sender, EventArgs e)
        {
            //Tester service = sender as Tester;
            ControlService service = sender as ControlService;
            Dispatcher.BeginInvoke((Action)(() =>
            {
                if (service.running)
                {
                    StartStopBtn.Content = "Stop";
                }
                else
                {
                    StartStopBtn.Content = "Start";
                }

                StartStopBtn.IsEnabled = true;
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
            //Tester service = root.rootHubtest;
            ControlService service = App.rootHub;
            await Task.Run(() =>
            {
                if (service.running)
                    service.Stop();
                else
                    service.Start();
            });
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
            dialog.InitialDirectory = Global.appdatapath;
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
                CompositeDeviceModel item = conLvViewModel.ControllerCol[idx];
                string prof = Global.ProfilePath[idx] = item.ProfileListCol[item.SelectedIndex].Name;
                if (item.LinkedProfile)
                {
                    Global.changeLinkedProfile(item.Device.getMacAddress(), Global.ProfilePath[idx]);
                    Global.SaveLinkedProfiles();
                }
                else
                {
                    Global.OlderProfilePath[idx] = Global.ProfilePath[idx];
                }

                //Global.Save();
                Global.LoadProfile(idx, true, App.rootHub);
                DS4Windows.AppLogger.LogToGui(Properties.Resources.UsingProfile.
                    Replace("*number*", (idx + 1).ToString()).Replace("*Profile name*", prof), false);
            }
        }

        private void CustomColorPick_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {

        }

        private void LightColorBtn_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            int idx = Convert.ToInt32(button.Tag);
            CompositeDeviceModel item = conLvViewModel.ControllerCol[idx];
            //(button.ContextMenu.Items[0] as MenuItem).IsChecked = conLvViewModel.ControllerCol[idx].UseCustomColor;
            //(button.ContextMenu.Items[1] as MenuItem).IsChecked = !conLvViewModel.ControllerCol[idx].UseCustomColor;
            button.ContextMenu = item.LightContext;
            button.ContextMenu.IsOpen = true;
        }

        private void MainDS4Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Application.Current.Shutdown();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            HookWindowMessages();
            HwndSource source = PresentationSource.FromVisual(this) as HwndSource;
            source.AddHook(WndProc);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam,
            IntPtr lParam, ref bool handled)
        {
            // Handle messages...
            switch (msg)
            {
                default: break;
            }

            return IntPtr.Zero;
        }

        private void HookWindowMessages()
        {
            Guid hidGuid = new Guid();
        }

        private void ProfEditSBtn_Click(object sender, RoutedEventArgs e)
        {
            Control temp = sender as Control;
            int idx = Convert.ToInt32(temp.Tag);
        }

        private void NewProfBtn_Click(object sender, RoutedEventArgs e)
        {
            Control temp = sender as Control;
            int idx = Convert.ToInt32(temp.Tag);
            controllerLV.SelectedIndex = idx;
            controllerLV.Focus();
        }

        private async void HideDS4ContCk_Click(object sender, RoutedEventArgs e)
        {
            StartStopBtn.IsEnabled = false;
            bool checkStatus = hideDS4ContCk.IsChecked == true;
            await Task.Run(() =>
            {
                App.rootHub.Stop();
                App.rootHub.Start();
            });

            StartStopBtn.IsEnabled = true;
        }

        private void UseUdpServerCk_Click(object sender, RoutedEventArgs e)
        {
            bool status = useUdpServerCk.IsChecked == true;
            udpServerTxt.IsEnabled = status;
            updPortNum.IsEnabled = status;
        }

        private void ProfFolderBtn_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(Global.appdatapath + "\\Profiles");
        }

        private void ControlPanelBtn_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("control", "joy.cpl");
        }

        private void DriverSetupBtn_Click(object sender, RoutedEventArgs e)
        {
            WelcomeDialog dialog = new WelcomeDialog();
            dialog.Owner = this;
            dialog.ShowDialog();
        }

        private void CheckUpdatesBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DriverSetup()
        {

        }

        private void UseWhiteDS4IconCk_Click(object sender, RoutedEventArgs e)
        {
            bool status = useWhiteDS4IconCk.IsChecked == true;
            notifyIcon.Icon = status ? Properties.Resources.DS4W___White : Properties.Resources.DS4W;
        }

        private void CheckDrivers()
        {
            bool deriverinstalled = Global.IsViGEmBusInstalled();
            if (!deriverinstalled)
            {
                Process p = new Process();
                p.StartInfo.FileName = $"{Global.exepath}\\DS4Windows.exe";
                p.StartInfo.Arguments = "driverinstall";
                p.StartInfo.Verb = "runas";
                try { p.Start(); }
                catch { }
            }
        }
    }
}
