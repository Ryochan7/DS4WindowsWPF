using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using DS4Windows;

namespace DS4WinWPF.DS4Forms.ViewModel
{
    public class ControllerListViewModel
    {
        private object _colLockobj = new object();
        private ObservableCollection<DS4Windows.DS4Device> controllerCol =
            new ObservableCollection<DS4Windows.DS4Device>();

        public ObservableCollection<DS4Device> ControllerCol
        { get => controllerCol; set => controllerCol = value; }

        public ControllerListViewModel(Tester tester)
        {
            tester.StartControllers += ControllersChanged;
            tester.PreRemoveControllers += ClearControllerList;
            IEnumerable<DS4Windows.DS4Device> devices =
                DS4Windows.DS4Devices.getDS4Controllers();
            foreach (DS4Windows.DS4Device currentDev in devices)
            {
                controllerCol.Add(currentDev);
                currentDev.Removal += Controller_Removal;
            }

            BindingOperations.EnableCollectionSynchronization(controllerCol, _colLockobj);
        }

        private void ClearControllerList(object sender, EventArgs e)
        {
            foreach (DS4Windows.DS4Device currentDev in controllerCol)
            {
                currentDev.Removal -= Controller_Removal;
            }

            controllerCol.Clear();
        }

        private void ControllersChanged(object sender, EventArgs e)
        {
            IEnumerable<DS4Windows.DS4Device> devices = DS4Windows.DS4Devices.getDS4Controllers();
            foreach (DS4Windows.DS4Device currentDev in devices)
            {
                if (!controllerCol.Contains(currentDev))
                {
                    controllerCol.Add(currentDev);
                    currentDev.Removal += Controller_Removal;
                }
            }
        }

        private void Controller_Removal(object sender, EventArgs e)
        {
            DS4Windows.DS4Device currentDev = sender as DS4Windows.DS4Device;
            controllerCol.Remove(currentDev);
        }
    }
}
