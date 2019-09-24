using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS4WinWPF.DS4Forms.ViewModel
{
    public class SettingsViewModel
    {
        public bool HideDS4Controller { get => DS4Windows.Global.UseExclusiveMode;
            set => DS4Windows.Global.UseExclusiveMode = value; }

        public bool SwipeTouchSwitchProfile { get => DS4Windows.Global.SwipeProfiles;
            set => DS4Windows.Global.SwipeProfiles = value; }

        private bool runAtStartup;
        public bool RunAtStartup { get => runAtStartup; set => runAtStartup = value; }

        private bool runStartProg;
        public bool RunStartProg { get => runStartProg; set => runStartProg = value; }

        private bool runStartTask;
        public bool RunStartTask { get => runStartTask; set => runStartTask = value; }

        public int ShowNotificationsIndex { get => DS4Windows.Global.Notifications; set => DS4Windows.Global.Notifications = value; }
        public bool DisconnectBTStop { get => DS4Windows.Global.DCBTatStop; set => DS4Windows.Global.DCBTatStop = value; }
        public bool FlashHighLatency { get => DS4Windows.Global.FlashWhenLate; set => DS4Windows.Global.FlashWhenLate = value; }
        public int FlashHighLatencyAt { get => DS4Windows.Global.FlashWhenLateAt; set => DS4Windows.Global.FlashWhenLateAt = value; }
        public bool StartMinimize { get => DS4Windows.Global.StartMinimized; set => DS4Windows.Global.StartMinimized = value; }
        public bool MinimizeToTaskbar { get => DS4Windows.Global.MinToTaskbar; set => DS4Windows.Global.MinToTaskbar = value; }
        public bool CloseMinimizes { get => DS4Windows.Global.CloseMini; set => DS4Windows.Global.CloseMini = value; }
        public bool QuickCharge { get => DS4Windows.Global.QuickCharge; set => DS4Windows.Global.QuickCharge = value; }
        public bool WhiteDS4Icon
        {
            get => DS4Windows.Global.UseWhiteIcon;
            set
            {
                if (DS4Windows.Global.UseWhiteIcon == value) return;
                DS4Windows.Global.UseWhiteIcon = value;
                WhiteDS4IconChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler WhiteDS4IconChanged;

        private bool checkForUpdates;
        public bool CheckForUpdates { get => checkForUpdates;
            set
            {
                if (checkForUpdates == value) return;
                checkForUpdates = value;
                CheckForUpdatesChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler CheckForUpdatesChanged;

        public int CheckEvery { get
            {
                int temp = DS4Windows.Global.CheckWhen;
                if (temp > 23)
                {
                    temp = temp / 24;
                }
                return temp;
            }
            set
            {
                if (checkEveryUnitIdx == 0 && value < 24)
                {
                    DS4Windows.Global.CheckWhen = value;
                }
                else if (checkEveryUnitIdx == 1)
                {
                    DS4Windows.Global.CheckWhen = value * 24;
                }
            }
        }
        public event EventHandler CheckEveryChanged;

        private int checkEveryUnitIdx = 1;
        public int CheckEveryUnit
        {
            get
            {
                return checkEveryUnitIdx;
            }
            set
            {
                if (checkEveryUnitIdx == value) return;
                checkEveryUnitIdx = value;
                CheckEveryUnitChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler CheckEveryUnitChanged;
        public bool UseUDPServer { get => DS4Windows.Global.isUsingUDPServer(); set => DS4Windows.Global.setUsingUDPServer(value); }
        public string UdpIpAddress { get => DS4Windows.Global.getUDPServerListenAddress();
            set => DS4Windows.Global.setUDPServerListenAddress(value); }
        public int UdpPort { get => DS4Windows.Global.getUDPServerPortNum(); set => DS4Windows.Global.setUDPServerPort(value); }
        public bool UseCustomSteamFolder { get => DS4Windows.Global.UseCustomSteamFolder;
            set => DS4Windows.Global.UseCustomSteamFolder = value; }
        public string CustomSteamFolder { get => DS4Windows.Global.CustomSteamFolder;
            set => DS4Windows.Global.CustomSteamFolder = value; }

        public SettingsViewModel()
        {
            checkForUpdates = DS4Windows.Global.CheckWhen > 0;
            checkEveryUnitIdx = 1;

            if (DS4Windows.Global.CheckWhen < 24)
            {
                checkEveryUnitIdx = 0;
            }

            CheckStartupOptiobs();
            CheckForUpdatesChanged += SettingsViewModel_CheckForUpdatesChanged;
        }

        private void SettingsViewModel_CheckForUpdatesChanged(object sender, EventArgs e)
        {
            if (!checkForUpdates)
            {
                CheckEveryChanged?.Invoke(this, EventArgs.Empty);
                CheckEveryUnitChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private void CheckStartupOptiobs()
        {
            bool lnkExists = File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Startup) + "\\DS4Windows.lnk");
            if (lnkExists)
            {
                runAtStartup = true;

            }
            else
            {
                runAtStartup = false;
            }
        }
    }
}
