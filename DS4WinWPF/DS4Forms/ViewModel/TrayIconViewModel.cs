using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Controls;
using DS4Windows;

namespace DS4WinWPF.DS4Forms.ViewModel
{
    public class TrayIconViewModel
    {
        private string tooltipText = "DS4Windows";
        private string iconSource = "/DS4WinWPF;component/Resources/DS4W.ico";
        public const string ballonTitle = "DS4Windows";
        public static string trayTitle = $"DS4Windows v{FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion}";
        private ContextMenu contextMenu;

        public string TooltipText { get => tooltipText;
            set
            {
                string temp = value;
                if (value.Length > 63) temp = value.Substring(0, 63);
                if (tooltipText == temp) return;
                tooltipText = temp;
                TooltipTextChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler TooltipTextChanged;

        public string IconSource { get => iconSource;
            set
            {
                if (iconSource == value) return;
                iconSource = value;
                IconSourceChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public ContextMenu ContextMenu { get => contextMenu; }

        public event EventHandler IconSourceChanged;
        public event EventHandler RequestShutdown;
        public event EventHandler RequestOpen;
        public event EventHandler RequestMinimize;

        private List<ControllerHolder> controllerList = new List<ControllerHolder>();
        private ProfileList profileListHolder;

        public delegate void ProfileSelectedHandler(TrayIconViewModel sender,
            ControllerHolder item, string profile);
        public event ProfileSelectedHandler ProfileSelected;

        //public TrayIconViewModel(Tester tester)
        public TrayIconViewModel(ControlService service, ProfileList profileListHolder)
        {
            this.profileListHolder = profileListHolder;
            contextMenu = new ContextMenu();
            PopulateControllerList();
            PopulateToolText();
            PopulateContextMenu();
            SetupBatteryUpdate();
            //profileListHolder.ProfileListCol.CollectionChanged += ProfileListCol_CollectionChanged;

            service.ServiceStarted += BuildControllerList;
            service.ServiceStarted += HookBatteryUpdate;
            service.ServiceStarted += StartPopulateText;
            service.PreServiceStop += ClearToolText;
            /*tester.StartControllers += HookBatteryUpdate;
            tester.StartControllers += StartPopulateText;
            tester.PreRemoveControllers += ClearToolText;
            tester.HotplugControllers += HookBatteryUpdate;
            tester.HotplugControllers += StartPopulateText;
            */
        }

        private void ProfileListCol_CollectionChanged(object sender,
            System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            PopulateContextMenu();
        }

        private void BuildControllerList(object sender, EventArgs e)
        {
            PopulateControllerList();
        }

        public void PopulateContextMenu()
        {
            contextMenu.Items.Clear();
            ItemCollection items = contextMenu.Items;
            MenuItem item;
            int idx = 0;

            foreach (ControllerHolder holder in controllerList)
            {
                DS4Device currentDev = holder.Device;
                item = new MenuItem() { Header = $"Controller {idx+1}" };
                item.Tag = idx;
                //item.ContextMenu = new ContextMenu();
                ItemCollection subitems = item.Items;
                string currentProfile = Global.ProfilePath[idx];
                foreach (ProfileEntity entry in profileListHolder.ProfileListCol)
                {
                    MenuItem temp = new MenuItem() { Header = entry.Name };
                    temp.Tag = idx;
                    temp.Click += ProfileItem_Click;
                    if (entry.Name == currentProfile)
                    {
                        temp.IsChecked = true;
                    }

                    subitems.Add(temp);
                }

                items.Add(item);
                idx++;
            }

            item = new MenuItem() { Header = "Disconnect Menu" };
            idx = 0;
            foreach (ControllerHolder holder in controllerList)
            {
                DS4Device tempDev = holder.Device;
                if (tempDev.Synced && !tempDev.Charging)
                {
                    MenuItem subitem = new MenuItem() { Header = $"Disconnect Controller {idx+1}" };
                    subitem.Click += DisconnectMenuItem_Click;
                    subitem.Tag = idx;
                    item.Items.Add(subitem);
                }

                idx++;
            }

            items.Add(item);
            items.Add(new Separator());
            item = new MenuItem() { Header = "Open" };
            item.Click += OpenMenuItem_Click;
            items.Add(item);
            item = new MenuItem() { Header = "Minimize" };
            item.Click += MinimizeMenuItem_Click;
            items.Add(item);
            items.Add(new Separator());
            item = new MenuItem() { Header = "Exit" };
            item.Click += ExitMenuItem_Click;
            items.Add(item);
        }

        private void OpenMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            RequestOpen?.Invoke(this, EventArgs.Empty);
        }

        private void MinimizeMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            RequestMinimize?.Invoke(this, EventArgs.Empty);
        }

        private void ProfileItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            int idx = Convert.ToInt32(item.Tag);
            ControllerHolder holder = controllerList[idx];
            ProfileSelected?.Invoke(this, holder, item.Header.ToString());
        }

        private void DisconnectMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            int idx = Convert.ToInt32(item.Tag);
            ControllerHolder holder = controllerList[idx];
            DS4Device tempDev = holder?.Device;
            if (tempDev != null && tempDev.Synced && !tempDev.Charging)
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

            //controllerList[idx] = null;
        }

        private void PopulateControllerList()
        {
            IEnumerable<DS4Device> devices = DS4Devices.getDS4Controllers();
            int idx = 0;
            foreach (DS4Device currentDev in devices)
            {
                controllerList.Add(new ControllerHolder(currentDev, idx));
                idx++;
            }
        }

        private void StartPopulateText(object sender, EventArgs e)
        {
            PopulateToolText();
            //PopulateContextMenu();
        }

        private void PopulateToolText()
        {
            List<string> items = new List<string>();
            items.Add(trayTitle);
            //IEnumerable<DS4Device> devices = DS4Devices.getDS4Controllers();
            int idx = 1;
            //foreach (DS4Device currentDev in devices)
            foreach (ControllerHolder holder in controllerList)
            {
                DS4Device currentDev = holder.Device;
                items.Add($"{idx}: {currentDev.ConnectionType} {currentDev.Battery}%{(currentDev.Charging ? "+" : "")}");
                idx++;
            }

            TooltipText = string.Join("\n", items);
        }

        private void SetupBatteryUpdate()
        {
            //IEnumerable<DS4Device> devices = DS4Devices.getDS4Controllers();
            //foreach (DS4Device currentDev in devices)
            foreach (ControllerHolder holder in controllerList)
            {
                DS4Device currentDev = holder.Device;
                currentDev.BatteryChanged += UpdateForBattery;
                currentDev.ChargingChanged += UpdateForBattery;
            }
        }

        private void HookBatteryUpdate(object sender, EventArgs e)
        {
            SetupBatteryUpdate();
        }

        private void UpdateForBattery(object sender, EventArgs e)
        {
            PopulateToolText();
        }

        private void ClearToolText(object sender, EventArgs e)
        {
            TooltipText = "DS4Windows";
            //contextMenu.Items.Clear();
        }

        private void ExitMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            RequestShutdown?.Invoke(this, EventArgs.Empty);
        }
    }

    public class ControllerHolder
    {
        private DS4Device device;
        private int index;
        public DS4Device Device { get => device; }
        public int Index { get => index; }

        public ControllerHolder(DS4Device device, int index)
        {
            this.device = device;
            this.index = index;
        }
    }
}
