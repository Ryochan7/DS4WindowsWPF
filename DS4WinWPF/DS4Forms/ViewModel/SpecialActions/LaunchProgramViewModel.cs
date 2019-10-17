using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS4Windows;

namespace DS4WinWPF.DS4Forms.ViewModel.SpecialActions
{
    public class LaunchProgramViewModel
    {
        private string filepath;
        private int delay;
        private string arguments;

        public string Filepath { get => filepath; set => filepath = value; }
        public int Delay { get => delay; set => delay = value; }
        public string Arguments { get => arguments; set => arguments = value; }

        public void LoadAction(SpecialAction action)
        {
            filepath = action.details;
            delay = (int)action.delayTime;
            arguments = action.extra;
        }

        public void SaveAction(SpecialAction action, bool edit = false)
        {
            Global.SaveAction(action.name, action.controls, 2, $"{filepath}?{delay}", edit, arguments);
        }
    }
}
