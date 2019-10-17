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
        private List<string> controlUnloadTriggerList = new List<string>();
        private bool editMode;


        public int DeviceNum { get => deviceNum; }
        public int ActionTypeIndex { get => actionTypeIndex; set => actionTypeIndex = value; }
        public string ActionName { get => actionName; set => actionName = value; }
        public SpecialAction.ActionTypeId[] TypeAssoc { get => typeAssoc; }
        public SpecialAction SavedAction { get => savedaction; }
        public List<string> ControlTriggerList { get => controlTriggerList; }
        public List<string> ControlUnloadTriggerList { get => controlUnloadTriggerList; }
        public bool EditMode { get => editMode; }

        public SpecialActEditorViewModel(int deviceNum, SpecialAction action)
        {
            this.deviceNum = deviceNum;
            savedaction = action;
            editMode = savedaction != null;
        }

        public void LoadAction(SpecialAction action)
        {
            foreach (string s in action.controls.Split('/'))
            {
                controlTriggerList.Add(s);
            }

            if (action.ucontrols != null)
            {
                foreach (string s in action.ucontrols.Split('/'))
                {
                    controlUnloadTriggerList.Add(s);
                }
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
            action.controls = string.Join("/", controlTriggerList.ToArray());
            if (controlUnloadTriggerList.Count > 0)
            {
                action.ucontrols = string.Join("/", controlUnloadTriggerList.ToArray());
            }
        }
    }
}
