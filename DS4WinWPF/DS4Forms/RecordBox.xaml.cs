using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using NonFormTimer = System.Timers.Timer;
using DS4WinWPF.DS4Forms.ViewModel;

namespace DS4WinWPF.DS4Forms
{
    /// <summary>
    /// Interaction logic for RecordBox.xaml
    /// </summary>
    public partial class RecordBox : UserControl
    {
        private RecordBoxViewModel recordBoxVM;
        private Stopwatch sw = new Stopwatch();
        private bool saved;
        public bool Saved { get => saved; }

        public event EventHandler Save;
        public event EventHandler Cancel;

        private Dictionary<int, bool> keysdownMap = new Dictionary<int, bool>();

        public RecordBox(int deviceNum, DS4Windows.DS4ControlSettings controlSettings, bool shift)
        {
            InitializeComponent();

            recordBoxVM = new RecordBoxViewModel(deviceNum, controlSettings, shift);
            mouseButtonsPanel.Visibility = Visibility.Hidden;
            extraConPanel.Visibility = Visibility.Hidden;

            DataContext = recordBoxVM;
        }

        private void SetupLateEvents()
        {
            macroListBox.SelectionChanged += MacroListBox_SelectionChanged;
        }

        private void MacroListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            saved = true;
            recordBoxVM.ExportMacro();
            Save?.Invoke(this, EventArgs.Empty);
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            Cancel?.Invoke(this, EventArgs.Empty);
        }

        private void RecordBtn_Click(object sender, RoutedEventArgs e)
        {
            bool recording = recordBoxVM.Recording = !recordBoxVM.Recording;
            if (recording)
            {
                DS4Windows.Program.rootHub.recordingMacro = true;
                recordBtn.Content = "Stop";
                mouseButtonsPanel.Visibility = Visibility.Visible;
                if (recordBoxVM.RecordDelays)
                {
                    extraConPanel.Visibility = Visibility.Hidden;
                }

                Enable_Controls(false);
                sw.Start();
                this.Focus();
            }
            else
            {
                DS4Windows.Program.rootHub.recordingMacro = false;
                recordBtn.Content = "Record";
                mouseButtonsPanel.Visibility = Visibility.Hidden;
                extraConPanel.Visibility = Visibility.Hidden;
                Enable_Controls(true);
                sw.Stop();
            }

            recordBoxVM.ToggleLightbar = false;
            recordBoxVM.ToggleRumble = false;
            changeLightBtn.Content = "Change Lightbar Color";
            addRumbleBtn.Content = "Add Rumble";
        }

        private void Enable_Controls(bool on)
        {
            macroListBox.IsEnabled = on;
            recordDelaysCk.IsEnabled = on;
            saveBtn.IsEnabled = on;
            cancelBtn.IsEnabled = on;
            loadPresetBtn.IsEnabled = on;
            savePresetBtn.IsEnabled = on;
            macroModeCombo.IsEnabled = on;
        }

        private void ChangeLightBtn_Click(object sender, RoutedEventArgs e)
        {
            bool light = recordBoxVM.ToggleLightbar = !recordBoxVM.ToggleLightbar;
            if (light)
            {
                changeLightBtn.Content = "Reset Lightbar Color";
            }
            else
            {
                changeLightBtn.Content = "Change Lightbar Color";
            }
        }

        private void AddRumbleBtn_Click(object sender, RoutedEventArgs e)
        {
            bool rumble = recordBoxVM.ToggleRumble = !recordBoxVM.ToggleRumble;
            if (rumble)
            {
                addRumbleBtn.Content = "Stop Rumble";
            }
            else
            {
                addRumbleBtn.Content = "Add Rumble";
            }
        }

        private void LoadPresetBtn_Click(object sender, RoutedEventArgs e)
        {
            loadPresetBtn.ContextMenu.IsOpen = true;
        }

        private void SavePresetBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void UserControl_KeyDown(object sender, KeyEventArgs e)
        {
            if (recordBoxVM.Recording)
            {
                int value = KeyInterop.VirtualKeyFromKey(e.Key);
                keysdownMap.TryGetValue(value, out bool isdown);
                if (!isdown)
                {
                    if (recordBoxVM.RecordDelays)
                    {
                        if (recordBoxVM.MacroSteps.Count > 0)
                        {
                            int elapsed = (int)sw.ElapsedMilliseconds + 300;
                            DS4Windows.MacroStep waitstep = new DS4Windows.MacroStep(elapsed, $"Wait {elapsed}",
                                DS4Windows.MacroStep.StepType.Wait, DS4Windows.MacroStep.StepOutput.None);
                            MacroStepItem waititem = new MacroStepItem(waitstep);
                            recordBoxVM.MacroSteps.Add(waititem);
                        }

                        sw.Restart();
                    }

                    DS4Windows.MacroStep step = new DS4Windows.MacroStep(KeyInterop.VirtualKeyFromKey(e.Key), e.Key.ToString(),
                            DS4Windows.MacroStep.StepType.ActDown, DS4Windows.MacroStep.StepOutput.Key);
                    MacroStepItem item = new MacroStepItem(step);
                    recordBoxVM.MacroSteps.Add(item);
                    keysdownMap.Add(value, true);
                }

                //Console.WriteLine(e.Key);
                //Console.WriteLine(e.SystemKey);
            }
        }

        private void UserControl_KeyUp(object sender, KeyEventArgs e)
        {
            if (recordBoxVM.Recording)
            {
                int value = KeyInterop.VirtualKeyFromKey(e.Key);
                keysdownMap.TryGetValue(value, out bool isdown);
                if (isdown)
                {
                    if (recordBoxVM.RecordDelays)
                    {
                        if (recordBoxVM.MacroSteps.Count > 0)
                        {
                            int elapsed = (int)sw.ElapsedMilliseconds + 300;
                            DS4Windows.MacroStep waitstep = new DS4Windows.MacroStep(elapsed, $"Wait {elapsed}",
                                DS4Windows.MacroStep.StepType.Wait, DS4Windows.MacroStep.StepOutput.None);
                            MacroStepItem waititem = new MacroStepItem(waitstep);
                            recordBoxVM.MacroSteps.Add(waititem);
                        }

                        sw.Restart();
                    }

                    DS4Windows.MacroStep step = new DS4Windows.MacroStep(KeyInterop.VirtualKeyFromKey(e.Key), e.Key.ToString(),
                            DS4Windows.MacroStep.StepType.ActUp, DS4Windows.MacroStep.StepOutput.Key);
                    MacroStepItem item = new MacroStepItem(step);
                    recordBoxVM.MacroSteps.Add(item);
                    keysdownMap.Remove(value);
                }

                //Console.WriteLine(e.Key);
                //Console.WriteLine(e.SystemKey);
            }
        }

        private void MacroListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (recordBoxVM.MacroStepIndex >= 0)
            {
                MacroStepItem item = recordBoxVM.MacroSteps[recordBoxVM.MacroStepIndex];
                if (item.Step.ActType == DS4Windows.MacroStep.StepType.Wait)
                {
                    ListBoxItem lbitem = macroListBox.ItemContainerGenerator.ContainerFromIndex(recordBoxVM.MacroStepIndex)
                        as ListBoxItem;
                    lbitem.ContentTemplate = this.FindResource("EditTemplate") as DataTemplate;
                }
            }
        }

        private void EditTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (recordBoxVM.MacroStepIndex >= 0)
            {
                ListBoxItem lbitem = macroListBox.ItemContainerGenerator.ContainerFromIndex(recordBoxVM.MacroStepIndex)
                        as ListBoxItem;
                lbitem.ContentTemplate = this.FindResource("DisplayTemplate") as DataTemplate;
            }
        }
    }
}
