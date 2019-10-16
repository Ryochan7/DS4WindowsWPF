using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS4Windows;

namespace DS4WinWPF.DS4Forms.ViewModel.SpecialActions
{
    public class DisconnectBTViewModel
    {
        private int holdInterval;
        public int HoldInterval { get => holdInterval; set => holdInterval = value; }

        public void LoadAction(SpecialAction action)
        {
            holdInterval = (int)action.delayTime;
        }

        public void SaveAction(SpecialAction action)
        {
            action.typeID = SpecialAction.ActionTypeId.DisconnectBT;
            action.type = "DisconnectBT";
            action.delayTime = holdInterval;
        }
    }
}
