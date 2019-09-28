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
using System.IO;

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
        private IntPtr regHandle = new IntPtr();
        private bool showInTaskbar = false;

        public MainWindow(ArgumentParser parser)
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
            trayIconVM = new TrayIconViewModel(App.rootHub, profileListHolder);
            notifyIcon.DataContext = trayIconVM;
            notifyIcon.Icon = Global.UseWhiteIcon ? Properties.Resources.DS4W___White :
                Properties.Resources.DS4W;

            if (Global.StartMinimized || parser.Mini)
            {
                WindowState = WindowState.Minimized;
            }

            bool isElevated = Global.IsAdministrator();
            if (isElevated)
            {
                uacImg.Visibility = Visibility.Collapsed;
            }

            this.Width = Global.FormWidth;
            this.Height = Global.FormHeight;
            WindowStartupLocation = WindowStartupLocation.Manual;
            Left = Global.FormLocationX;
            Top = Global.FormLocationY;

            SetupEvents();

            Task.Run(() =>
            {
                CheckDrivers();
                App.rootHub.Start();
                //root.rootHubtest.Start();
            });
        }

        private void TrayIconVM_RequestMinimize(object sender, EventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void TrayIconVM_ProfileSelected(TrayIconViewModel sender,
            ControllerHolder item, string profile)
        {
            int idx = item.Index;
            CompositeDeviceModel devitem = conLvViewModel.ControllerDict[idx];
            if (devitem != null)
            {
                devitem.ChangeSelectedProfile(profile);
            }
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
            App.rootHub.PreServiceStop += PrepareForServiceStop;
            //root.rootHubtest.RunningChanged += ControlServiceChanged;
            conLvViewModel.ControllerCol.CollectionChanged += ControllerCol_CollectionChanged;
            DS4Windows.AppLogger.TrayIconLog += ShowNotification;
            DS4Windows.AppLogger.GuiLog += UpdateLastStatusMessage;
            App.rootHub.Debug += UpdateLastStatusMessage;
            trayIconVM.RequestShutdown += TrayIconVM_RequestShutdown;
            trayIconVM.ProfileSelected += TrayIconVM_ProfileSelected;
            trayIconVM.RequestMinimize += TrayIconVM_RequestMinimize;
            trayIconVM.RequestOpen += TrayIconVM_RequestOpen;
        }

        private void PrepareForServiceStop(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                trayIconVM.ClearContextMenu();
            }));
        }

        private void TrayIconVM_RequestOpen(object sender, EventArgs e)
        {
            WindowState = WindowState.Normal;
            if (showInTaskbar)
                Show();
        }

        private void TrayIconVM_RequestShutdown(object sender, EventArgs e)
        {
            this.Close();
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
                System.Collections.IList newitems = e.NewItems;
                if (newitems != null)
                {
                    foreach (CompositeDeviceModel item in newitems)
                    {
                        item.LightContext = new ContextMenu();
                        item.AddLightContextItems();
                        item.Device.SyncChange += DS4Device_SyncChange;
                        //item.LightContext.Items.Add(new MenuItem() { Header = "Use Profile Color", IsChecked = !item.UseCustomColor });
                        //item.LightContext.Items.Add(new MenuItem() { Header = "Use Custom Color", IsChecked = item.UseCustomColor });
                    }
                }

                if (App.rootHub.running)
                    trayIconVM.PopulateContextMenu();
            }));
        }

        private void DS4Device_SyncChange(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                trayIconVM.PopulateContextMenu();
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
                CompositeDeviceModel item = conLvViewModel.ControllerDict[idx];
                item.RequestUpdatedTooltipID();
            }
        }

        /// <summary>
        /// Clear and re-populate tray context menu
        /// </summary>
        private void NotifyIcon_TrayRightMouseUp(object sender, RoutedEventArgs e)
        {
            notifyIcon.ContextMenu = trayIconVM.ContextMenu;
        }

        /// <summary>
        /// Change profile based on selection
        /// </summary>
        private void SelectProfCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox box = sender as ComboBox;
            int idx = Convert.ToInt32(box.Tag);
            if (idx > -1 && conLvViewModel.ControllerDict.ContainsKey(idx))
            {
                CompositeDeviceModel item = conLvViewModel.ControllerDict[idx];
                if (item.SelectedIndex > -1)
                {
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
                    trayIconVM.PopulateContextMenu();
                }
            }
        }

        private void CustomColorPick_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {

        }

        private void LightColorBtn_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            int idx = Convert.ToInt32(button.Tag);
            CompositeDeviceModel item = conLvViewModel.ControllerDict[idx];
            //(button.ContextMenu.Items[0] as MenuItem).IsChecked = conLvViewModel.ControllerCol[idx].UseCustomColor;
            //(button.ContextMenu.Items[1] as MenuItem).IsChecked = !conLvViewModel.ControllerCol[idx].UseCustomColor;
            button.ContextMenu = item.LightContext;
            button.ContextMenu.IsOpen = true;
        }

        private void MainDS4Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Util.UnregisterNotify(regHandle);
            Application.Current.Shutdown();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            HwndSource source = PresentationSource.FromVisual(this) as HwndSource;
            HookWindowMessages(source);
            source.AddHook(WndProc);
        }

        private bool inHotPlug = false;
        private int hotplugCounter = 0;
        private object hotplugCounterLock = new object();
        private const int DBT_DEVNODES_CHANGED = 0x0007;
        private const int DBT_DEVICEARRIVAL = 0x8000;
        private const int DBT_DEVICEREMOVECOMPLETE = 0x8004;
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam,
            IntPtr lParam, ref bool handled)
        {
            // Handle messages...
            switch (msg)
            {
                case Util.WM_DEVICECHANGE:
                {
                    if (Global.runHotPlug)
                    {
                        Int32 Type = wParam.ToInt32();
                        if (Type == DBT_DEVICEARRIVAL ||
                            Type == DBT_DEVICEREMOVECOMPLETE)
                        {
                            lock (hotplugCounterLock)
                            {
                                hotplugCounter++;
                            }

                            if (!inHotPlug)
                            {
                                inHotPlug = true;
                                Task.Run(() => { InnerHotplug2(); });
                            }
                        }
                    }
                    break;
                }
                default: break;
            }

            return IntPtr.Zero;
        }

        private void InnerHotplug2()
        {
            inHotPlug = true;

            bool loopHotplug = false;
            lock (hotplugCounterLock)
            {
                loopHotplug = hotplugCounter > 0;
            }

            while (loopHotplug == true)
            {
                Thread.Sleep(1500);
                Program.rootHub.HotPlug();
                //TaskRunner.Run(() => { Program.rootHub.HotPlug(uiContext); });
                lock (hotplugCounterLock)
                {
                    hotplugCounter--;
                    loopHotplug = hotplugCounter > 0;
                }
            }

            inHotPlug = false;
        }

        private void HookWindowMessages(HwndSource source)
        {
            Guid hidGuid = new Guid();
            NativeMethods.HidD_GetHidGuid(ref hidGuid);
            bool result = Util.RegisterNotify(source.Handle, hidGuid, ref regHandle);
            if (!result)
            {
                App.Current.Shutdown();
            }
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

        private async void UseUdpServerCk_Click(object sender, RoutedEventArgs e)
        {
            bool status = useUdpServerCk.IsChecked == true;
            udpServerTxt.IsEnabled = status;
            updPortNum.IsEnabled = status;
            if (!status)
            {
                App.rootHub.ChangeMotionEventStatus(status);
                await Task.Delay(100).ContinueWith((t) =>
                {
                    App.rootHub.ChangeUDPStatus(status);
                });
            }
            else
            {
                Program.rootHub.ChangeUDPStatus(status);
                await Task.Delay(100).ContinueWith((t) =>
                {
                    App.rootHub.ChangeMotionEventStatus(status);
                });
            }
        }

        private void ProfFolderBtn_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(Global.appdatapath + "\\Profiles");
        }

        private void ControlPanelBtn_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("control", "joy.cpl");
        }

        private async void DriverSetupBtn_Click(object sender, RoutedEventArgs e)
        {
            StartStopBtn.IsEnabled = false;
            await Task.Run(() =>
            {
                if (App.rootHub.running)
                    App.rootHub.Stop();
            });

            StartStopBtn.IsEnabled = true;
            WelcomeDialog dialog = new WelcomeDialog();
            dialog.Owner = this;
            dialog.ShowDialog();
        }

        private void CheckUpdatesBtn_Click(object sender, RoutedEventArgs e)
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
                p.StartInfo.FileName = $"{Global.exelocation}";
                p.StartInfo.Arguments = "-driverinstall";
                p.StartInfo.Verb = "runas";
                try { p.Start(); }
                catch { }
            }
        }

        private void ImportProfBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.AddExtension = true;
            dialog.DefaultExt = ".xml";
            dialog.Filter = "DS4Windows Profile (*.xml)|*.xml";
            dialog.Title = "Select Profile to Import File";
            if (Global.appdatapath != Global.exepath)
                dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\DS4Windows" + @"\Profiles\";
            else
                dialog.InitialDirectory = Global.exepath + @"\Profiles\";

            if (dialog.ShowDialog() == true)
            {
                string[] files = dialog.FileNames;
                for (int i = 0, arlen = files.Length; i < arlen; i++)
                {
                    string profilename = System.IO.Path.GetFileName(files[i]);
                    string basename = System.IO.Path.GetFileNameWithoutExtension(files[i]);
                    File.Copy(dialog.FileNames[i], Global.appdatapath + "\\Profiles\\" + profilename, true);
                    profileListHolder.AddProfileSort(basename);
                }
            }
        }

        private void ExportProfBtn_Click(object sender, RoutedEventArgs e)
        {
            if (profilesListBox.SelectedIndex >= 0)
            {
                SaveFileDialog dialog = new SaveFileDialog();
                dialog.AddExtension = true;
                dialog.DefaultExt = ".xml";
                dialog.Filter = "DS4Windows Profile (*.xml)|*.xml";
                dialog.Title = "Select Profile to Export File";
                Stream stream;
                int idx = profilesListBox.SelectedIndex;
                Stream profile = new StreamReader(Global.appdatapath + "\\Profiles\\" + profileListHolder.ProfileListCol[idx].Name + ".xml").BaseStream;
                if (dialog.ShowDialog() == true)
                {
                    if ((stream = dialog.OpenFile()) != null)
                    {
                        profile.CopyTo(stream);
                        profile.Close();
                        stream.Close();
                    }
                }
            }
        }

        private void DupProfBtn_Click(object sender, RoutedEventArgs e)
        {
            string filename = "";
            if (profilesListBox.SelectedIndex >= 0)
            {
                int idx = profilesListBox.SelectedIndex;
                filename = profileListHolder.ProfileListCol[idx].Name;
                dupBox.OldFilename = filename;
                dupBoxBar.Visibility = Visibility.Visible;
                dupBox.Save += (sender2, profilename) =>
                {
                    profileListHolder.AddProfileSort(profilename);
                    dupBoxBar.Visibility = Visibility.Collapsed;
                };
                dupBox.Cancel += (sender2, args) =>
                {
                    dupBoxBar.Visibility = Visibility.Collapsed;
                };
            }
        }

        private void DeleteProfBtn_Click(object sender, RoutedEventArgs e)
        {
            if (profilesListBox.SelectedIndex >= 0)
            {
                int idx = profilesListBox.SelectedIndex;
                string filename = profileListHolder.ProfileListCol[idx].Name;
                if (MessageBox.Show(Properties.Resources.ProfileCannotRestore.Replace("*Profile name*", "\"" + filename + "\""),
                    Properties.Resources.DeleteProfile,
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    File.Delete(Global.appdatapath + @"\Profiles\" + filename + ".xml");
                    profileListHolder.ProfileListCol.RemoveAt(idx);
                }
            }
        }

        private void SelectProfCombo_KeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
        }

        private void MainDS4Window_StateChanged(object sender, EventArgs e)
        {
            bool minToTask = Global.MinToTaskbar;
            if (WindowState == WindowState.Minimized && !minToTask)
            {
                Hide();
                showInTaskbar = false;
            }
            else if (WindowState == WindowState.Normal && !minToTask)
            {
                Show();
                showInTaskbar = true;
            }
        }

        private void MainDS4Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (WindowState != WindowState.Minimized)
            {
                Global.FormWidth = Convert.ToInt32(Width);
                Global.FormHeight = Convert.ToInt32(Height);

            }
        }

        private void MainDS4Window_LocationChanged(object sender, EventArgs e)
        {
            if (WindowState != WindowState.Minimized)
            {
                Global.FormLocationX = Convert.ToInt32(Left);
                Global.FormLocationY = Convert.ToInt32(Top);
            }
        }
    }
}
