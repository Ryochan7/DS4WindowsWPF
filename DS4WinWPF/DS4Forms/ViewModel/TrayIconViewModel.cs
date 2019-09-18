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

        public event EventHandler IconSourceChanged;

        public TrayIconViewModel(Tester tester)
        {
            PopulateToolText();
            SetupBatteryUpdate();

            tester.StartControllers += HookBatteryUpdate;
            tester.StartControllers += StartPopulateText;
            tester.PreRemoveControllers += ClearToolText;
            tester.HotplugControllers += HookBatteryUpdate;
            tester.HotplugControllers += StartPopulateText;
        }

        private void StartPopulateText(object sender, EventArgs e)
        {
            PopulateToolText();
        }

        private void PopulateToolText()
        {
            List<string> items = new List<string>();
            items.Add(trayTitle);
            IEnumerable<DS4Device> devices = DS4Devices.getDS4Controllers();
            int idx = 1;
            foreach (DS4Device currentDev in devices)
            {
                items.Add($"{idx}: {currentDev.ConnectionType} {currentDev.Battery}%{(currentDev.Charging ? "+" : "")}");
            }

            TooltipText = string.Join("\n", items);
        }

        private void SetupBatteryUpdate()
        {
            IEnumerable<DS4Device> devices = DS4Devices.getDS4Controllers();
            foreach (DS4Device currentDev in devices)
            {
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
        }

        public void BuildContextMenu(ItemCollection collection, MainWindow mainwin)
        {
            collection.Clear();
            collection.Add(new MenuItem { Header = "Open" });
            collection.Add(new Separator());
            MenuItem temp = new MenuItem { Header = "Exit" };
            temp.Click += ExitMenuItem_Click;
            collection.Add(temp);
        }

        private void ExitMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            App.Current.Shutdown();
        }
    }
}
