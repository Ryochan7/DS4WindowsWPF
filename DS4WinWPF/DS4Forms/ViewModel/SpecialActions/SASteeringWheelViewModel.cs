using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS4Windows;

namespace DS4WinWPF.DS4Forms.ViewModel.SpecialActions
{
    public class SASteeringWheelViewModel
    {
        private int delay;
        public int Delay { get => delay; set => delay = value; }

        public void LoadAction(SpecialAction action)
        {
            delay = (int)action.delayTime;
        }

        public void SaveAction(SpecialAction action, bool edit = false)
        {
            Global.SaveAction(action.name, action.controls, 7, delay.ToString(), edit);
        }
    }
}
