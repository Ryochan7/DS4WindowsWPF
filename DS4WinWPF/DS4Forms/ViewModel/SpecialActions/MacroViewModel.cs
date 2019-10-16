using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS4Windows;

namespace DS4WinWPF.DS4Forms.ViewModel.SpecialActions
{
    public class MacroViewModel
    {
        private bool useScanCode;
        private bool runTriggerRelease;
        private bool syncRun;
        private bool keepKeyState;
        private bool repeatHeld;
        private int macro;
        private string macrostring;

        public bool UseScanCode { get => useScanCode; set => useScanCode = value; }
        public bool RunTriggerRelease { get => runTriggerRelease; set => runTriggerRelease = value; }
        public bool SyncRun { get => syncRun; set => syncRun = value; }
        public bool KeepKeyState { get => keepKeyState; set => keepKeyState = value; }
        public bool RepeatHeld { get => repeatHeld; set => repeatHeld = value; }
        public int Macro { get => macro; set => macro = value; }
        public string Macrostring { get => macrostring; set => macrostring = value; }

        public void LoadAction(SpecialAction action)
        {

        }

        public void SaveAction(SpecialAction action)
        {

        }
    }
}
