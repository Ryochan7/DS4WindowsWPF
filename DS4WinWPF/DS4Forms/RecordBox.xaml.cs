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
using NonFormTimer = System.Timers.Timer;
using DS4WinWPF.DS4Forms.ViewModel;
using Microsoft.Win32;

namespace DS4WinWPF.DS4Forms
{
    /// <summary>
    /// Interaction logic for RecordBox.xaml
    /// </summary>
    public partial class RecordBox : UserControl
    {
        private RecordBoxViewModel recordBoxVM;
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
                recordBoxVM.Sw.Restart();
                this.Focus();
            }
            else
            {
                DS4Windows.Program.rootHub.recordingMacro = false;
                recordBtn.Content = "Record";
                mouseButtonsPanel.Visibility = Visibility.Hidden;
                extraConPanel.Visibility = Visibility.Hidden;
                Enable_Controls(true);
                recordBoxVM.Sw.Stop();
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
            macroListBox.ItemsSource = null;
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.AddExtension = true;
            dialog.DefaultExt = ".txt";
            dialog.Filter = "Text Documents (*.txt)|*.txt";
            dialog.Title = "Select Export File";
            dialog.InitialDirectory = $"{DS4Windows.Global.appdatapath}\\Macros";
            if (dialog.ShowDialog() == true)
            {
                //recordBoxVM.MacroSteps.Clear();
                recordBoxVM.SavePreset(dialog.FileName);
            }

            macroListBox.ItemsSource = recordBoxVM.MacroSteps;
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
                            int elapsed = (int)recordBoxVM.Sw.ElapsedMilliseconds + 300;
                            DS4Windows.MacroStep waitstep = new DS4Windows.MacroStep(elapsed, $"Wait {elapsed-300}",
                                DS4Windows.MacroStep.StepType.Wait, DS4Windows.MacroStep.StepOutput.None);
                            MacroStepItem waititem = new MacroStepItem(waitstep);
                            recordBoxVM.MacroSteps.Add(waititem);
                        }

                        recordBoxVM.Sw.Restart();
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
                            int elapsed = (int)recordBoxVM.Sw.ElapsedMilliseconds + 300;
                            DS4Windows.MacroStep waitstep = new DS4Windows.MacroStep(elapsed, $"Wait {elapsed-300}",
                                DS4Windows.MacroStep.StepType.Wait, DS4Windows.MacroStep.StepOutput.None);
                            MacroStepItem waititem = new MacroStepItem(waitstep);
                            recordBoxVM.MacroSteps.Add(waititem);
                        }

                        recordBoxVM.Sw.Restart();
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

        private void CycleProgPresetMenuItem_Click(object sender, RoutedEventArgs e)
        {
            macroListBox.ItemsSource = null;
            recordBoxVM.MacroSteps.Clear();
            recordBoxVM.WriteCycleProgramsPreset();
            macroListBox.ItemsSource = recordBoxVM.MacroSteps;
        }

        private void LoadPresetFromFileMenuItem_Click(object sender, RoutedEventArgs e)
        {
            macroListBox.ItemsSource = null;
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.AddExtension = true;
            dialog.DefaultExt = ".txt";
            dialog.Filter = "Text Documents (*.txt)|*.txt";
            dialog.Title = "Select Preset File";
            dialog.InitialDirectory = $"{DS4Windows.Global.appdatapath}\\Macros";
            if (dialog.ShowDialog() == true)
            {
                recordBoxVM.MacroSteps.Clear();
                recordBoxVM.LoadPresetFromFile(dialog.FileName);
            }

            macroListBox.ItemsSource = recordBoxVM.MacroSteps;
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                FocusNavigationDirection focusDirection = FocusNavigationDirection.Next;
                TraversalRequest request = new TraversalRequest(focusDirection);
                UIElement elementWithFocus = Keyboard.FocusedElement as UIElement;
                elementWithFocus?.MoveFocus(request);
            }
        }

        private void FourMouseBtn_Click(object sender, RoutedEventArgs e)
        {
            int value = 259;
            keysdownMap.TryGetValue(value, out bool isdown);
            if (!isdown)
            {
                DS4Windows.MacroStep step = new DS4Windows.MacroStep(value, DS4Windows.MacroParser.macroInputNames[value],
                            DS4Windows.MacroStep.StepType.ActDown, DS4Windows.MacroStep.StepOutput.Button);
                recordBoxVM.AddMacroStep(step);
                keysdownMap.Add(value, true);
            }
            else
            {
                DS4Windows.MacroStep step = new DS4Windows.MacroStep(value, DS4Windows.MacroParser.macroInputNames[value],
                            DS4Windows.MacroStep.StepType.ActUp, DS4Windows.MacroStep.StepOutput.Button);
                recordBoxVM.AddMacroStep(step);
                keysdownMap.Remove(value);
            }
        }

        private void FiveMouseBtn_Click(object sender, RoutedEventArgs e)
        {
            int value = 260;
            keysdownMap.TryGetValue(value, out bool isdown);
            if (!isdown)
            {
                DS4Windows.MacroStep step = new DS4Windows.MacroStep(value, DS4Windows.MacroParser.macroInputNames[value],
                            DS4Windows.MacroStep.StepType.ActDown, DS4Windows.MacroStep.StepOutput.Button);
                recordBoxVM.AddMacroStep(step);
                keysdownMap.Add(value, true);
            }
            else
            {
                DS4Windows.MacroStep step = new DS4Windows.MacroStep(value, DS4Windows.MacroParser.macroInputNames[value],
                            DS4Windows.MacroStep.StepType.ActUp, DS4Windows.MacroStep.StepOutput.Button);
                recordBoxVM.AddMacroStep(step);
                keysdownMap.Remove(value);
            }
        }

        private void UserControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (recordBoxVM.Recording)
            {
                int value;
                switch (e.ChangedButton)
                {
                    case MouseButton.Left: value = 256; break;
                    case MouseButton.Right: value = 257; break;
                    case MouseButton.Middle: value = 258; break;
                    case MouseButton.XButton1: value = 259; break;
                    case MouseButton.XButton2: value = 260; break;
                    default: value = 0; break;
                }

                DS4Windows.MacroStep step = new DS4Windows.MacroStep(value, DS4Windows.MacroParser.macroInputNames[value],
                            DS4Windows.MacroStep.StepType.ActDown, DS4Windows.MacroStep.StepOutput.Button);
                recordBoxVM.AddMacroStep(step);
                keysdownMap.Add(value, true);
            }
        }

        private void UserControl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (recordBoxVM.Recording)
            {
                int value;
                switch (e.ChangedButton)
                {
                    case MouseButton.Left: value = 256; break;
                    case MouseButton.Right: value = 257; break;
                    case MouseButton.Middle: value = 258; break;
                    case MouseButton.XButton1: value = 259; break;
                    case MouseButton.XButton2: value = 260; break;
                    default: value = 0; break;
                }

                DS4Windows.MacroStep step = new DS4Windows.MacroStep(value, DS4Windows.MacroParser.macroInputNames[value],
                            DS4Windows.MacroStep.StepType.ActUp, DS4Windows.MacroStep.StepOutput.Button);
                recordBoxVM.AddMacroStep(step);
                keysdownMap.Remove(value);
            }
        }
    }
}
