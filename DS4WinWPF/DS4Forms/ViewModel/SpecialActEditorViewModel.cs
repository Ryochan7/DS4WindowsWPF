using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS4Windows;

namespace DS4WinWPF.DS4Forms.ViewModel
{
    public class SpecialActEditorViewModel
    {
        private int deviceNum;
        private int actionTypeIndex = 0;
        private string actionName;
        private SpecialAction savedaction;
        private SpecialAction.ActionTypeId[] typeAssoc = new SpecialAction.ActionTypeId[]
        {
            SpecialAction.ActionTypeId.None, SpecialAction.ActionTypeId.Macro,
            SpecialAction.ActionTypeId.Program, SpecialAction.ActionTypeId.Profile,
            SpecialAction.ActionTypeId.Key, SpecialAction.ActionTypeId.DisconnectBT,
            SpecialAction.ActionTypeId.BatteryCheck, SpecialAction.ActionTypeId.MultiAction,
            SpecialAction.ActionTypeId.SASteeringWheelEmulationCalibrate,
        };

        private List<string> controlTriggerList = new List<string>();

        public int DeviceNum { get => deviceNum; }
        public int ActionTypeIndex { get => actionTypeIndex; set => actionTypeIndex = value; }
        public string ActionName { get => actionName; set => actionName = value; }
        public SpecialAction.ActionTypeId[] TypeAssoc { get => typeAssoc; }
        public SpecialAction SavedAction { get => savedaction; }
        public List<string> ControlTriggerList { get => controlTriggerList; }

        public SpecialActEditorViewModel(int deviceNum, SpecialAction action)
        {
            this.deviceNum = deviceNum;
            savedaction = action;
        }

        public void LoadAction(SpecialAction action)
        {
            foreach (string s in action.controls.Split('/'))
            {
                controlTriggerList.Add(s);
            }

            actionName = action.name;
            for(int i = 0; i < typeAssoc.Length; i++)
            {
                SpecialAction.ActionTypeId type = typeAssoc[i];
                if (type == action.typeID)
                {
                    actionTypeIndex = i;
                    break;
                }
            }
        }

        public void SetAction(SpecialAction action)
        {
            action.name = actionName;
        }
    }
}
