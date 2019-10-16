using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DS4WinWPF.DS4Forms.ViewModel;
using DS4WinWPF.DS4Forms.ViewModel.SpecialActions;

namespace DS4WinWPF.DS4Forms
{
    /// <summary>
    /// Interaction logic for SpecialActionEditor.xaml
    /// </summary>
    public partial class SpecialActionEditor : UserControl
    {
        private SpecialActEditorViewModel specialActVM;
        private MacroViewModel macroActVM;
        private DisconnectBTViewModel disconnectBtVM;

        public event EventHandler Cancel;
        public event EventHandler Saved;

        public SpecialActionEditor(int deviceNum, DS4Windows.SpecialAction specialAction = null)
        {
            InitializeComponent();

            specialActVM = new SpecialActEditorViewModel(deviceNum, specialAction);
            macroActVM = new MacroViewModel();
            disconnectBtVM = new DisconnectBTViewModel();

            actionTypeTabControl.DataContext = specialActVM;
            actionTypeCombo.DataContext = specialActVM;
            disconnectBTTab.DataContext = disconnectBtVM;

            // Hide tab headers. Tab content will still be visible
            blankActTab.Visibility = Visibility.Collapsed;
            macroActTab.Visibility = Visibility.Collapsed;
            launchProgActTab.Visibility = Visibility.Collapsed;
            loadProfileTab.Visibility = Visibility.Collapsed;
            pressKetActTab.Visibility = Visibility.Collapsed;
            disconnectBTTab.Visibility = Visibility.Collapsed;
            checkBatteryTab.Visibility = Visibility.Collapsed;
            multiActTab.Visibility = Visibility.Collapsed;
            sixaxisWheelCalibrateTab.Visibility = Visibility.Collapsed;

            if (specialAction != null)
            {
                LoadAction(specialAction);
            }

            SetupLateEvents();
        }

        private void SetupLateEvents()
        {
            actionTypeCombo.SelectionChanged += ActionTypeCombo_SelectionChanged;
        }

        private void LoadAction(DS4Windows.SpecialAction specialAction)
        {
            specialActVM.LoadAction(specialAction);

            switch (specialAction.typeID)
            {
                case DS4Windows.SpecialAction.ActionTypeId.Macro:
                    macroActVM.LoadAction(specialAction);
                    break;
                case DS4Windows.SpecialAction.ActionTypeId.DisconnectBT:
                    disconnectBtVM.LoadAction(specialAction); break;
            }
        }

        private void ActionTypeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (specialActVM.ActionTypeIndex <= 0)
            {
                saveBtn.IsEnabled = false;
            }
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            Cancel?.Invoke(this, EventArgs.Empty);
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            DS4Windows.SpecialAction.ActionTypeId typeId = specialActVM.TypeAssoc[specialActVM.ActionTypeIndex];
            DS4Windows.SpecialAction tempAct = new DS4Windows.SpecialAction("null", "null", "null", "null");
            specialActVM.SetAction(tempAct);
            switch (typeId)
            {
                case DS4Windows.SpecialAction.ActionTypeId.Macro:
                    macroActVM.SaveAction(tempAct);
                    break;
                case DS4Windows.SpecialAction.ActionTypeId.DisconnectBT:
                    disconnectBtVM.SaveAction(tempAct);
                    break;
            }

            Saved?.Invoke(this, EventArgs.Empty);
        }

        private void ControlTriggerCheckBox_Click(object sender, RoutedEventArgs e)
        {
            CheckBox check = sender as CheckBox;
            string name = check.Tag.ToString();
            if (check.IsChecked == true)
            {
                specialActVM.ControlTriggerList.Add(name);
            }
            else
            {
                specialActVM.ControlTriggerList.Remove(name);
            }
        }
    }
}
