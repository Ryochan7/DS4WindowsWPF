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
        private List<CheckBox> triggerBoxes;
        private List<CheckBox> unloadTriggerBoxes;

        private SpecialActEditorViewModel specialActVM;
        private MacroViewModel macroActVM;
        private LaunchProgramViewModel launchProgVM;
        private LoadProfileViewModel loadProfileVM;
        private PressKeyViewModel pressKeyVM;
        private DisconnectBTViewModel disconnectBtVM;
        private SASteeringWheelViewModel saSteeringWheelVM;

        public event EventHandler Cancel;
        public delegate void SaveHandler(object sender, string actionName);
        public event SaveHandler Saved;

        public SpecialActionEditor(int deviceNum, DS4Windows.SpecialAction specialAction = null)
        {
            InitializeComponent();

            triggerBoxes = new List<CheckBox>()
            {
                crossTrigCk, circleTrigCk, squareTrigCk, triangleTrigCk,
                optionsTrigCk, shareTrigCk, upTrigCk, downTrigCk,
                leftTrigCk, rightTrigCk, psTrigCk, l1TrigCk,
                r1TrigCk, l2TrigCk, r2TrigCk, l3TrigCk,
                r3TrigCk, leftTouchTrigCk, upperTouchTrigCk, multitouchTrigCk,
                rightTouchTrigCk, lsuTrigCk, lsdTrigCk, lslTrigCk,
                lsrTrigCk, rsuTrigCk, rsdTrigCk, rslTrigCk,
                rsrTrigCk, swipeUpTrigCk, swipeDownTrigCk, swipeLeftTrigCk,
                swipeRightTrigCk, tiltUpTrigCk, tiltDownTrigCk, tiltLeftTrigCk,
                tiltRightTrigCk,
            };

            unloadTriggerBoxes = new List<CheckBox>()
            {
                unloadCrossTrigCk, unloadCircleTrigCk, unloadSquareTrigCk, unloadTriangleTrigCk,
                unloadOptionsTrigCk, unloadShareTrigCk, unloadUpTrigCk, unloadDownTrigCk,
                unloadLeftTrigCk, unloadRightTrigCk, unloadPsTrigCk, unloadL1TrigCk,
                unloadR1TrigCk, unloadL2TrigCk, unloadR2TrigCk, unloadL3TrigCk,
                unloadR3TrigCk, unloadLeftTouchTrigCk, unloadUpperTouchTrigCk, unloadMultitouchTrigCk,
                unloadRightTouchTrigCk, unloadLsuTrigCk, unloadLsdTrigCk, unloadLslTrigCk,
                unloadLsrTrigCk, unloadRsuTrigCk, unloadRsdTrigCk, unloadRslTrigCk,
                unloadRsrTrigCk, unloadSwipeUpTrigCk, unloadSwipeDownTrigCk, unloadSwipeLeftTrigCk,
                unloadSwipeRightTrigCk, unloadTiltUpTrigCk, unloadTiltDownTrigCk, unloadTiltLeftTrigCk,
                unloadTiltRightTrigCk,
            };

            specialActVM = new SpecialActEditorViewModel(deviceNum, specialAction);
            macroActVM = new MacroViewModel();
            launchProgVM = new LaunchProgramViewModel();
            loadProfileVM = new LoadProfileViewModel();
            pressKeyVM = new PressKeyViewModel();
            disconnectBtVM = new DisconnectBTViewModel();
            saSteeringWheelVM = new SASteeringWheelViewModel();

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

            actionTypeTabControl.DataContext = specialActVM;
            actionTypeCombo.DataContext = specialActVM;
            macroActTab.DataContext = macroActVM;
            launchProgActTab.DataContext = launchProgVM;
            //loadProfileTab.DataContext = loadProfileVM;
            pressKetActTab.DataContext = pressKeyVM;
            disconnectBTTab.DataContext = disconnectBtVM;
            sixaxisWheelCalibrateTab.DataContext = saSteeringWheelVM;

            SetupLateEvents();
        }

        private void SetupLateEvents()
        {
            actionTypeCombo.SelectionChanged += ActionTypeCombo_SelectionChanged;
        }

        private void LoadAction(DS4Windows.SpecialAction specialAction)
        {
            specialActVM.LoadAction(specialAction);
            foreach(string control in specialActVM.ControlTriggerList)
            {
                foreach(CheckBox box in triggerBoxes)
                {
                    if (box.Tag.ToString() == control)
                    {
                        box.IsChecked = true;
                        break;
                    }
                }
            }

            foreach (string control in specialActVM.ControlUnloadTriggerList)
            {
                foreach (CheckBox box in unloadTriggerBoxes)
                {
                    if (box.Tag.ToString() == control)
                    {
                        box.IsChecked = true;
                        break;
                    }
                }
            }

            switch (specialAction.typeID)
            {
                case DS4Windows.SpecialAction.ActionTypeId.Macro:
                    macroActVM.LoadAction(specialAction);
                    break;
                case DS4Windows.SpecialAction.ActionTypeId.Program:
                    launchProgVM.LoadAction(specialAction);
                    break;
                case DS4Windows.SpecialAction.ActionTypeId.Profile:
                    loadProfileVM.LoadAction(specialAction);
                    break;
                case DS4Windows.SpecialAction.ActionTypeId.DisconnectBT:
                    disconnectBtVM.LoadAction(specialAction);
                    break;
                case DS4Windows.SpecialAction.ActionTypeId.SASteeringWheelEmulationCalibrate:
                    saSteeringWheelVM.LoadAction(specialAction);
                    break;
            }
        }

        private void ActionTypeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (specialActVM.ActionTypeIndex <= 0)
            {
                saveBtn.IsEnabled = false;
            }

            triggersListBox.Visibility = Visibility.Visible;
            unloadTriggersListBox.Visibility = Visibility.Collapsed;
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            Cancel?.Invoke(this, EventArgs.Empty);
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            DS4Windows.SpecialAction.ActionTypeId typeId = specialActVM.TypeAssoc[specialActVM.ActionTypeIndex];
            DS4Windows.SpecialAction tempAct = new DS4Windows.SpecialAction("null", "null", "null", "null");
            bool editMode = specialActVM.EditMode;
            if (editMode && specialActVM.SavedAction.name != specialActVM.ActionName)
            {
                DS4Windows.Global.RemoveAction(specialActVM.SavedAction.name);
                editMode = false;
            }

            specialActVM.SetAction(tempAct);
            switch (typeId)
            {
                case DS4Windows.SpecialAction.ActionTypeId.Macro:
                    macroActVM.SaveAction(tempAct, editMode);
                    break;
                case DS4Windows.SpecialAction.ActionTypeId.Program:
                    launchProgVM.SaveAction(tempAct, editMode);
                    break;
                case DS4Windows.SpecialAction.ActionTypeId.Profile:
                    loadProfileVM.SaveAction(tempAct, editMode);
                    break;
                case DS4Windows.SpecialAction.ActionTypeId.DisconnectBT:
                    disconnectBtVM.SaveAction(tempAct, editMode);
                    break;
                case DS4Windows.SpecialAction.ActionTypeId.SASteeringWheelEmulationCalibrate:
                    saSteeringWheelVM.SaveAction(tempAct, editMode);
                    break;
            }

            Saved?.Invoke(this, tempAct.name);
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

        private void ControlUnloadTriggerCheckBox_Click(object sender, RoutedEventArgs e)
        {
            CheckBox check = sender as CheckBox;
            string name = check.Tag.ToString();
            if (check.IsChecked == true)
            {
                specialActVM.ControlUnloadTriggerList.Add(name);
            }
            else
            {
                specialActVM.ControlUnloadTriggerList.Remove(name);
            }
        }

        private void RecordMacroBtn_Click(object sender, RoutedEventArgs e)
        {
            DS4Windows.DS4ControlSettings settings = macroActVM.PrepareSettings();
            RecordBoxWindow recordWin = new RecordBoxWindow(specialActVM.DeviceNum, settings);
            recordWin.Saved += (sender2, args) =>
            {
                macroActVM.Macro.Clear();
                macroActVM.Macro.AddRange((int[])settings.action);
                macroActVM.Macrostring = string.Join(", ", macroActVM.Macro);
            };

            recordWin.ShowDialog();
        }

        private void PressKeyToggleTriggerBtn_Click(object sender, RoutedEventArgs e)
        {
            bool normalTrigger = !pressKeyVM.NormalTrigger;
            if (normalTrigger)
            {
                pressKeyToggleTriggerBtn.Content = "Set Unload Trigger";
                triggersListBox.Visibility = Visibility.Visible;
                unloadTriggersListBox.Visibility = Visibility.Collapsed;
            }
            else
            {
                pressKeyToggleTriggerBtn.Content = "Set Regular Trigger";
                triggersListBox.Visibility = Visibility.Collapsed;
                unloadTriggersListBox.Visibility = Visibility.Visible;
            }
        }

        private void LoadProfUnloadBtn_Click(object sender, RoutedEventArgs e)
        {
            triggersListBox.Visibility = Visibility.Collapsed;
            unloadTriggersListBox.Visibility = Visibility.Visible;
        }
    }
}
