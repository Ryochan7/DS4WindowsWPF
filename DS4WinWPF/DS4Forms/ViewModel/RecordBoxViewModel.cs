using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS4Windows;

namespace DS4WinWPF.DS4Forms.ViewModel
{
    public class RecordBoxViewModel
    {
        private int deviceNum;
        public int DeviceNum { get => deviceNum; }

        private DS4ControlSettings settings;
        public DS4ControlSettings Settings { get => settings; }

        private bool shift;
        public bool Shift { get => shift; }

        private bool recordDelays;
        public bool RecordDelays { get => recordDelays; set => recordDelays = value; }

        private int macroModeIndex;
        public int MacroModeIndex { get => macroModeIndex; set => macroModeIndex = value; }

        private bool recording;
        public bool Recording { get => recording; set => recording = value; }

        private bool toggleLightbar;
        public bool ToggleLightbar { get => toggleLightbar; set => toggleLightbar = value; }

        private bool toggleRummble;
        public bool ToggleRumble { get => toggleRummble; set => toggleRummble = value; }
        
        private ObservableCollection<MacroStepItem> macroSteps =
            new ObservableCollection<MacroStepItem>();
        public ObservableCollection<MacroStepItem> MacroSteps { get => macroSteps; }
        
        private int macroStepIndex;
        public int MacroStepIndex { get => macroStepIndex; set => macroStepIndex = value; }

        public RecordBoxViewModel(int deviceNum, DS4ControlSettings controlSettings, bool shift)
        {
            this.deviceNum = deviceNum;
            settings = controlSettings;
            this.shift = shift;
            if (settings.keyType.HasFlag(DS4KeyType.HoldMacro))
            {
                macroModeIndex = 1;
            }

            if (!shift && settings.action is int[])
            {
                LoadMacro();
            }
            else if (shift && settings.shiftAction is int[])
            {
                LoadMacro();
            }
        }

        public void LoadMacro()
        {
            int[] macro;
            if (!shift)
            {
                macro = (int[])settings.action;
            }
            else
            {
                macro = (int[])settings.shiftAction;
            }

            MacroParser macroParser = new MacroParser(macro);
            macroParser.LoadMacro();
            foreach(MacroStep step in macroParser.MacroSteps)
            {
                MacroStepItem item = new MacroStepItem(step);
                macroSteps.Add(item);
            }
        }

        public void ExportMacro()
        {
            int[] outmac = new int[macroSteps.Count];
            int index = 0;
            foreach(MacroStepItem step in macroSteps)
            {
                outmac[index] = step.Step.Value;
                index++;
            }

            if (!shift)
            {
                settings.action = outmac;
                settings.actionType = DS4ControlSettings.ActionType.Macro;
                settings.keyType = DS4KeyType.Macro;
                if (macroModeIndex == 1)
                {
                    settings.keyType |= DS4KeyType.HoldMacro;
                }
            }
            else
            {
                settings.shiftAction = outmac;
                settings.shiftActionType = DS4ControlSettings.ActionType.Macro;
                settings.shiftKeyType = DS4KeyType.Macro;
                if (macroModeIndex == 1)
                {
                    settings.shiftKeyType |= DS4KeyType.HoldMacro;
                }
            }
        }
    }

    public class MacroStepItem
    {
        private static string[] imageSources = new string[]
        {
            "/DS4WinWPF;component/Resources/DS4 Config.png",
            "/DS4WinWPF;component/Resources/DS4 Config.png",
            "/DS4WinWPF;component/Resources/DS4 Config.png",
        };

        private MacroStep step;
        private string image;

        public string Image { get => image; }
        public MacroStep Step { get => step; }

        public MacroStepItem(MacroStep step)
        {
            this.step = step;
            image = imageSources[(int)step.ActType];
        }
    }
}
