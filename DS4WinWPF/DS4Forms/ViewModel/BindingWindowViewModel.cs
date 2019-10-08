using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS4Windows;

namespace DS4WinWPF.DS4Forms.ViewModel
{
    public class BindingWindowViewModel
    {
        private int deviceNum;
        private bool use360Mode;
        private MappedControl mappedControl;
        private OutBinding currentOutBind;
        private OutBinding shiftOutBind;
        private OutBinding actionBinding;

        public bool Using360Mode
        {
            get => use360Mode;
        }
        public int DeviceNum { get => deviceNum; }
        public MappedControl MappedControl { get => mappedControl; }
        public OutBinding CurrentOutBind { get => currentOutBind; }
        public OutBinding ShiftOutBind { get => shiftOutBind; }
        public OutBinding ActionBinding
        {
            get => actionBinding;
            set
            {
                actionBinding = value;
            }
        }

        public BindingWindowViewModel(int deviceNum, MappedControl mappedControl)
        {
            this.deviceNum = deviceNum;
            use360Mode = Global.outDevTypeTemp[deviceNum] == OutContType.X360;
            this.mappedControl = mappedControl;
            currentOutBind = new OutBinding();
            shiftOutBind = new OutBinding();
            PopulateCurrentBinds();
        }

        private void PopulateCurrentBinds()
        {
            DS4ControlSettings setting = mappedControl.Setting;
            bool sc = setting.keyType.HasFlag(DS4KeyType.ScanCode);
            currentOutBind.input = setting.control;
            shiftOutBind.input = setting.control;
            if (setting.action != null)
            {
                switch(setting.actionType)
                {
                    case DS4ControlSettings.ActionType.Button:
                        currentOutBind.outputType = OutBinding.OutType.Button;
                        currentOutBind.control = (X360Controls)setting.action;
                        break;
                    case DS4ControlSettings.ActionType.Default:
                        currentOutBind.outputType = OutBinding.OutType.Default;
                        break;
                    case DS4ControlSettings.ActionType.Key:
                        currentOutBind.outputType = OutBinding.OutType.Key;
                        currentOutBind.outkey = (int)setting.action;
                        currentOutBind.hasScanCode = sc;

                        break;
                    case DS4ControlSettings.ActionType.Macro:
                        currentOutBind.outputType = OutBinding.OutType.Macro;
                        currentOutBind.macro = (int[])setting.action;
                        break;
                }
            }
            else
            {
                currentOutBind.outputType = OutBinding.OutType.Default;
            }

            if (setting.shiftAction != null)
            {
                sc = setting.shiftKeyType.HasFlag(DS4KeyType.ScanCode);
                switch (setting.shiftAction)
                {
                    case DS4ControlSettings.ActionType.Button:
                        shiftOutBind.shiftAction = true;
                        shiftOutBind.shiftTrigger = setting.shiftTrigger;
                        shiftOutBind.outputType = OutBinding.OutType.Button;
                        shiftOutBind.control = (X360Controls)setting.shiftAction;
                        break;
                    case DS4ControlSettings.ActionType.Default:
                        shiftOutBind.shiftAction = true;
                        shiftOutBind.shiftTrigger = setting.shiftTrigger;
                        shiftOutBind.outputType = OutBinding.OutType.Default;
                        break;
                    case DS4ControlSettings.ActionType.Key:
                        shiftOutBind.shiftAction = true;
                        shiftOutBind.shiftTrigger = setting.shiftTrigger;
                        shiftOutBind.outputType = OutBinding.OutType.Key;
                        shiftOutBind.outkey = (int)setting.shiftAction;
                        shiftOutBind.hasScanCode = sc;
                        break;
                    case DS4ControlSettings.ActionType.Macro:
                        shiftOutBind.shiftAction = true;
                        shiftOutBind.shiftTrigger = setting.shiftTrigger;
                        shiftOutBind.outputType = OutBinding.OutType.Macro;
                        shiftOutBind.macro = (int[])setting.shiftAction;
                        break;
                }
            }
        }
    }

    public class OutBinding
    {
        public enum OutType : uint
        {
            Default,
            Key,
            Button,
            Macro
        }

        public DS4Controls input;
        public bool toggle;
        public bool hasScanCode;
        public OutType outputType;
        public int outkey;
        public int[] macro;
        public X360Controls control;
        public bool shiftAction;
        public int shiftTrigger;

        public bool HasScanCode { get => hasScanCode; }
        public bool Toggle { get => toggle; }
        public int ShiftTrigger { get => shiftTrigger; }

        public bool IsShift()
        {
            return shiftAction;
        }

        public bool IsMouse()
        {
            return outputType == OutType.Button && (control >= X360Controls.LeftMouse && control < X360Controls.Unbound);
        }

        public static bool IsMouseRange(X360Controls control)
        {
            return control >= X360Controls.LeftMouse && control < X360Controls.Unbound;
        }
    }
}
