using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS4Windows;

namespace DS4WinWPF.DS4Forms.ViewModel.SpecialActions
{
    public class MacroViewModel
    {
        private bool useScanCode;
        private bool runTriggerRelease;
        private bool syncRun;
        private bool keepKeyState;
        private bool repeatHeld;
        private List<int> macro;
        private string macrostring;

        public bool UseScanCode { get => useScanCode; set => useScanCode = value; }
        public bool RunTriggerRelease { get => runTriggerRelease; set => runTriggerRelease = value; }
        public bool SyncRun { get => syncRun; set => syncRun = value; }
        public bool KeepKeyState { get => keepKeyState; set => keepKeyState = value; }
        public bool RepeatHeld { get => repeatHeld; set => repeatHeld = value; }
        public List<int> Macro { get => macro; set => macro = value; }
        public string Macrostring { get => macrostring;
            set
            {
                macrostring = value;
                MacrostringChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler MacrostringChanged;

        public void LoadAction(SpecialAction action)
        {
            macro = action.macro;
            macrostring = string.Join(", ", action.macro.ToString());
            useScanCode = action.keyType.HasFlag(DS4KeyType.ScanCode);
            runTriggerRelease = action.pressRelease;
            syncRun = action.synchronized;
            keepKeyState = action.keepKeyState;
            repeatHeld = action.keyType.HasFlag(DS4KeyType.RepeatMacro);
        }

        public DS4ControlSettings PrepareSettings()
        {
            DS4ControlSettings settings = new DS4ControlSettings(DS4Controls.None);
            settings.action = macro;
            settings.actionType = DS4ControlSettings.ActionType.Macro;
            settings.keyType = DS4KeyType.Macro;
            if (repeatHeld)
            {
                settings.keyType |= DS4KeyType.RepeatMacro;
            }

            return settings;
        }

        public void SaveAction(SpecialAction action, bool edit = false)
        {
            List<string> extrasList = new List<string>();
            extrasList.Add(useScanCode ? "Scan Code" : null);
            extrasList.Add(runTriggerRelease ? "RunOnRelease" : null);
            extrasList.Add(syncRun ? "Sync" : null);
            extrasList.Add(keepKeyState ? "KeepKeyState" : null);
            extrasList.Add(repeatHeld ? "Repeat" : null);
            Global.SaveAction(action.name, action.controls, 1, string.Join("/", macro), edit,
                string.Join("/", extrasList));
        }
    }
}
