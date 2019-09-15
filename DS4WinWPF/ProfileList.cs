using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
            profileListCol.Add(new ProfileEntity { Name = "Doom 3 BFG" });
            profileListCol.Add(new ProfileEntity { Name = "Turok 2" });
            BindingOperations.EnableCollectionSynchronization(profileListCol, _proLockobj);
        }

        
    }
}
