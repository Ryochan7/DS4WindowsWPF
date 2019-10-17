using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS4Windows;

namespace DS4WinWPF.DS4Forms.ViewModel.SpecialActions
{
    public class LoadProfileViewModel
    {
        private string profilename;
        private bool autoUntrigger;

        public void LoadAction(SpecialAction action)
        {
            profilename = action.details;
            autoUntrigger = action.automaticUntrigger;
        }

        public void SaveAction(SpecialAction action, bool edit = false)
        {
            Global.SaveAction(action.name, action.controls, 3, profilename, edit,
                action.ucontrols +
                (autoUntrigger ? (action.ucontrols.Length > 0 ? "/" : "") + "AutomaticUntrigger" : ""));
        }
    }
}
