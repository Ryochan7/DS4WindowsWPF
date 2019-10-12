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
        
        private ObservableCollection<MacroStep> macroSteps =
            new ObservableCollection<MacroStep>();
        public ObservableCollection<MacroStep> MacroSteps { get => macroSteps; }
        
        private int macroStepIndex;
        public int MacroStepIndex { get => macroStepIndex; set => macroStepIndex = value; }

        public RecordBoxViewModel(int deviceNum, DS4ControlSettings controlSettings)
        {
            this.deviceNum = deviceNum;
            settings = controlSettings;
            if (settings.keyType.HasFlag(DS4KeyType.HoldMacro))
            {
                macroModeIndex = 1;
            }
        }
    }

    public class MacroStep
    {
        public enum StepType : uint
        {
            ActDown,
            ActUp,
            Wait,
        }

        private string name;
        private int value;
        private StepType type;
        private string image;

        public string Name { get => name; set => name = value; }
        public int Value { get => value; set => this.value = value; }
        public StepType Type { get => type; set => type = value; }
        public string Image { get => image; set => image = value; }
    }
}
