using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using DS4Windows;

namespace DS4WinWPF.DS4Forms.ViewModel
{
    public class RecordBoxViewModel
    {
        private Stopwatch sw = new Stopwatch();
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

        private bool toggle4thMouse;
        private bool toggle5thMouse;
        private int appendIndex = -1;

        
        private ObservableCollection<MacroStepItem> macroSteps =
            new ObservableCollection<MacroStepItem>();
        public ObservableCollection<MacroStepItem> MacroSteps { get => macroSteps; }
        
        private int macroStepIndex;
        public int MacroStepIndex { get => macroStepIndex; set => macroStepIndex = value; }
        public Stopwatch Sw { get => sw; }
        public bool Toggle4thMouse { get => toggle4thMouse; set => toggle4thMouse = value; }
        public bool Toggle5thMouse { get => toggle5thMouse; set => toggle5thMouse = value; }
        public int AppendIndex { get => appendIndex; set => appendIndex = value; }

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

        public void WriteCycleProgramsPreset()
        {
            MacroStep step = new MacroStep(18, KeyInterop.KeyFromVirtualKey(18).ToString(),
                MacroStep.StepType.ActDown, MacroStep.StepOutput.Key);
            macroSteps.Add(new MacroStepItem(step));

            step = new MacroStep(9, KeyInterop.KeyFromVirtualKey(9).ToString(),
                MacroStep.StepType.ActDown, MacroStep.StepOutput.Key);
            macroSteps.Add(new MacroStepItem(step));

            step = new MacroStep(9, KeyInterop.KeyFromVirtualKey(9).ToString(),
                MacroStep.StepType.ActUp, MacroStep.StepOutput.Key);
            macroSteps.Add(new MacroStepItem(step));

            step = new MacroStep(18, KeyInterop.KeyFromVirtualKey(18).ToString(),
                MacroStep.StepType.ActUp, MacroStep.StepOutput.Key);
            macroSteps.Add(new MacroStepItem(step));

            step = new MacroStep(1300, $"Wait 1000",
                MacroStep.StepType.Wait, MacroStep.StepOutput.None);
            macroSteps.Add(new MacroStepItem(step));
        }

        public void LoadPresetFromFile(string filepath)
        {
            string[] macs = File.ReadAllText(filepath).Split('/');
            List<int> tmpmacro = new List<int>();
            int temp;
            foreach (string s in macs)
            {
                if (int.TryParse(s, out temp))
                    tmpmacro.Add(temp);
            }

            MacroParser macroParser = new MacroParser(tmpmacro.ToArray());
            macroParser.LoadMacro();
            foreach (MacroStep step in macroParser.MacroSteps)
            {
                MacroStepItem item = new MacroStepItem(step);
                macroSteps.Add(item);
            }
        }

        public void SavePreset(string filepath)
        {
            int[] outmac = new int[macroSteps.Count];
            int index = 0;
            foreach (MacroStepItem step in macroSteps)
            {
                outmac[index] = step.Step.Value;
                index++;
            }

            string macro = string.Join("/", outmac);
            StreamWriter sw = new StreamWriter(filepath);
            sw.Write(macro);
            sw.Close();
        }

        public void AddMacroStep(MacroStep step)
        {
            if (recordDelays && macroSteps.Count > 0)
            {
                int elapsed = (int)sw.ElapsedMilliseconds + 300;
                MacroStep waitstep = new MacroStep(elapsed, $"Wait {elapsed - 300}",
                    MacroStep.StepType.Wait, MacroStep.StepOutput.None);
                MacroStepItem waititem = new MacroStepItem(waitstep);
                if (appendIndex == -1)
                {
                    macroSteps.Add(waititem);
                }
                else
                {
                    macroSteps.Insert(appendIndex, waititem);
                    appendIndex++;
                }
            }

            sw.Restart();
            MacroStepItem item = new MacroStepItem(step);
            if (appendIndex == -1)
            {
                macroSteps.Add(item);
            }
            else
            {
                macroSteps.Insert(appendIndex, item);
                appendIndex++;
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
        public int DisplayValue
        {
            get
            {
                int result = step.Value;
                if (step.ActType == MacroStep.StepType.Wait)
                {
                    result -= 300;
                }

                return result;
            }
            set
            {
                int result = value;
                if (step.ActType == MacroStep.StepType.Wait)
                {
                    result += 300;
                }

                step.Value = result;
            }
        }

        public MacroStepItem(MacroStep step)
        {
            this.step = step;
            image = imageSources[(int)step.ActType];
        }
    }
}
