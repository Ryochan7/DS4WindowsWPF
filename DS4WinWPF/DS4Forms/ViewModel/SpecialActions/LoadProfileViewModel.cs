using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS4Windows;

namespace DS4WinWPF.DS4Forms.ViewModel.SpecialActions
{
    public class LoadProfileViewModel
    {
        private bool autoUntrigger;
        private ProfileList profileList;
        private int profileIndex;

        public bool AutoUntrigger { get => autoUntrigger; set => autoUntrigger = value; }
        public int ProfileIndex
        {
            get => profileIndex;
            set
            {
                if (profileIndex == value) return;
                profileIndex = value;
                ProfileIndexChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler ProfileIndexChanged;

        public bool UnloadEnabled { get => profileIndex > 0; }
        public event EventHandler UnloadEnabledChanged;

        public ProfileList ProfileList { get => profileList; }

        public LoadProfileViewModel(ProfileList profileList)
        {
            this.profileList = profileList;

            ProfileIndexChanged += LoadProfileViewModel_ProfileIndexChanged;
        }

        public void LoadAction(SpecialAction action)
        {
            autoUntrigger = action.automaticUntrigger;
            string profilename = action.details;
            ProfileEntity item = profileList.ProfileListCol.Single(x => x.Name == profilename);
            if (item != null)
            {
                profileIndex = profileList.ProfileListCol.IndexOf(item) + 1;
            }
        }

        private void LoadProfileViewModel_ProfileIndexChanged(object sender, EventArgs e)
        {
            UnloadEnabledChanged?.Invoke(this, EventArgs.Empty);
        }

        public void SaveAction(SpecialAction action, bool edit = false)
        {
            if (profileIndex > 0)
            {
                string profilename = profileList.ProfileListCol[profileIndex - 1].Name;
                Global.SaveAction(action.name, action.controls, 3, profilename, edit,
                    action.ucontrols +
                    (autoUntrigger ? (action.ucontrols.Length > 0 ? "/" : "") + "AutomaticUntrigger" : ""));
            }
        }
    }
}
