﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace DS4WinWPF
{
    public class ProfileList
    {
        private object _proLockobj = new object();
        private ObservableCollection<ProfileEntity> profileListCol =
            new ObservableCollection<ProfileEntity>();

        public ObservableCollection<ProfileEntity> ProfileListCol { get => profileListCol; set => profileListCol = value; }

        public ProfileList()
        {
            BindingOperations.EnableCollectionSynchronization(profileListCol, _proLockobj);
        }

        public void Refresh()
        {
            profileListCol.Clear();
            string[] profiles = Directory.GetFiles(DS4Windows.Global.appdatapath + @"\Profiles\");
            foreach (string s in profiles)
            {
                if (s.EndsWith(".xml"))
                {
                    ProfileEntity item = new ProfileEntity()
                    {
                        Name = Path.GetFileNameWithoutExtension(s)
                    };

                    profileListCol.Add(item);
                }
            }
        }
    }
}
