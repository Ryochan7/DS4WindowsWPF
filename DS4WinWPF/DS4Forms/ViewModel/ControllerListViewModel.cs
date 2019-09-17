using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Data;
using DS4Windows;

namespace DS4WinWPF.DS4Forms.ViewModel
{
    public class ControllerListViewModel
    {
        private object _colLockobj = new object();
        private ObservableCollection<CompositeDeviceModel> controllerCol =
            new ObservableCollection<CompositeDeviceModel>();

        public ObservableCollection<CompositeDeviceModel> ControllerCol
        { get => controllerCol; set => controllerCol = value; }

        private ProfileList profileListHolder;
        private int currentIndex;
        public int CurrentIndex { get => currentIndex; set => currentIndex = value; }
        public CompositeDeviceModel CurrentItem => controllerCol[currentIndex];

        public ControllerListViewModel(Tester tester, ProfileList profileListHolder)
        {
            this.profileListHolder = profileListHolder;
            tester.StartControllers += ControllersChanged;
            tester.ControllersRemoved += ClearControllerList;
            IEnumerable<DS4Device> devices =
                DS4Devices.getDS4Controllers();
            int idx = 0;
            foreach (DS4Device currentDev in devices)
            {
                CompositeDeviceModel temp = new CompositeDeviceModel(currentDev,
                    idx, "Turok 2", profileListHolder);
                controllerCol.Add(temp);
                currentDev.Removal += Controller_Removal;
                idx++;
            }

            BindingOperations.EnableCollectionSynchronization(controllerCol, _colLockobj);
        }

        private void ClearControllerList(object sender, EventArgs e)
        {
            foreach (CompositeDeviceModel temp in controllerCol)
            {
                temp.Device.Removal -= Controller_Removal;
            }

            controllerCol.Clear();
        }

        private void ControllersChanged(object sender, EventArgs e)
        {
            IEnumerable<DS4Windows.DS4Device> devices = DS4Windows.DS4Devices.getDS4Controllers();
            foreach (DS4Windows.DS4Device currentDev in devices)
            {
                bool found = false;
                foreach(CompositeDeviceModel temp in controllerCol)
                {
                    if (temp.Device == currentDev)
                    {
                        found = true;
                        break;
                    }
                }


                if (!found)
                {
                    CompositeDeviceModel temp = new CompositeDeviceModel(currentDev,
                        controllerCol.Count, "Doom 3 BFG", profileListHolder);
                    controllerCol.Add(temp);
                    currentDev.Removal += Controller_Removal;
                }
            }
        }

        private void Controller_Removal(object sender, EventArgs e)
        {
            DS4Device currentDev = sender as DS4Device;
            foreach (CompositeDeviceModel temp in controllerCol)
            {
                if (temp.Device == currentDev)
                {
                    controllerCol.Remove(temp);
                    break;
                }
            }
        }
    }

    public class CompositeDeviceModel
    {
        private DS4Device device;
        private string selectedProfile;
        private ProfileList profileListHolder;
        private ProfileEntity selectedEntity;
        private int selectedIndex = 1;
        private int devIndex;

        public DS4Device Device { get => device; set => device = value; }
        public string SelectedProfile { get => selectedProfile; set => selectedProfile = value; }
        public ProfileList ProfileEntities { get => profileListHolder; set => profileListHolder = value; }
        public ObservableCollection<ProfileEntity> ProfileListCol => profileListHolder.ProfileListCol;

        public string LightColor
        {
            get
            {
                DS4Color color = device.LightBarColor;
                return $"#{color.red.ToString("X2")}{color.green.ToString("X2")}{color.blue.ToString("X2")}";
            }
        }

        public ProfileEntity SelectedEntity { get => selectedEntity; set => selectedEntity = value; }
        public int SelectedIndex { get => selectedIndex; set => selectedIndex = value; }

        public string StatusSource
        {
            get
            {
                string source = device.ConnectionType == ConnectionType.USB ? "/DS4WinWPF;component/Resources/USB.png"
                    : "/DS4WinWPF;component/Resources/BT.png";
                return source;
            }
        }

        public string ExclusiveSource
        {
            get
            {
                string source = device.IsExclusive ? "/DS4WinWPF;component/Resources/checked.png" :
                    "/DS4WinWPF;component/Resources/cancel.png";
                return source;
            }
        }

        public bool LinkedProfile
        {
            get
            {
                return false;
            }
        }

        public int DevIndex { get => devIndex; }

        public CompositeDeviceModel(DS4Device device, int devIndex, string profile,
            ProfileList collection)
        {
            this.device = device;
            this.devIndex = devIndex;
            this.selectedProfile = profile;
            profileListHolder = collection;
            this.selectedEntity = profileListHolder.ProfileListCol.Single(x => x.Name == selectedProfile);
        }
    }
}
