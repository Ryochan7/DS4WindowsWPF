using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using DS4Windows;

namespace DS4WinWPF.DS4Forms.ViewModel.SpecialActions
{
    public class PressKeyViewModel
    {
        private string describeText;
        private DS4KeyType keyType;
        private int value;
        private int pressReleaseIndex = 0;
        private bool normalTrigger = true;

        public string DescribeText
        {
            get
            {
                string result = "Select a Key";
                if (!string.IsNullOrEmpty(describeText))
                {
                    result = describeText;
                };

                return result;
            }
        }
        public event EventHandler DescribeTextChanged;
        public DS4KeyType KeyType { get => keyType; set => keyType = value; }
        public int Value { get => value; set => this.value = value; }
        public int PressReleaseIndex { get => pressReleaseIndex; set => pressReleaseIndex = value; }
        public bool NormalTrigger { get => normalTrigger; set => normalTrigger = value; }

        public void LoadAction(SpecialAction action)
        {
            keyType = action.keyType;
            if (!string.IsNullOrEmpty(action.ucontrols))
            {
                keyType |= DS4KeyType.Toggle;
            }

            int.TryParse(action.details, out value);

            if (action.pressRelease)
            {
                pressReleaseIndex = 1;
            }

            UpdateDescribeText();
        }

        public void UpdateDescribeText()
        {
            describeText = KeyInterop.KeyFromVirtualKey(value).ToString() +
                (keyType.HasFlag(DS4KeyType.ScanCode) ? "(SC)" : "") +
                (keyType.HasFlag(DS4KeyType.Toggle) ? "(Toggle)" : "");

            DescribeTextChanged?.Invoke(this, EventArgs.Empty);
        }

        public DS4ControlSettings PrepareSettings()
        {
            DS4ControlSettings settings = new DS4ControlSettings(DS4Controls.None);
            settings.action = value;
            settings.keyType = keyType;
            settings.actionType = DS4ControlSettings.ActionType.Key;
            return settings;
        }

        public void ReadSettings(DS4ControlSettings settings)
        {
            value = (int)settings.action;
            keyType = settings.keyType;
        }

        public void SaveAction(SpecialAction action, bool edit = false)
        {
            string uaction = null;
            if (keyType.HasFlag(DS4KeyType.Toggle))
            {
                uaction = "Press";
                if (pressReleaseIndex == 1)
                {
                    uaction = "Release";
                }
            }

            Global.SaveAction(action.name, action.controls, 4,
                $"{value}{(keyType.HasFlag(DS4KeyType.ScanCode) ? " Scan Code" : "")}", edit,
                !string.IsNullOrEmpty(uaction) ? $"{uaction}\n{action.ucontrols}" : "");
        }
    }
}
