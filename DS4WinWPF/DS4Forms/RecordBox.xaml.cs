﻿using System;
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
using Microsoft.Win32;
using Xceed.Wpf.Toolkit;
using DS4WinWPF.DS4Forms.ViewModel;

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

        private ColorPickerWindow colorDialog;
        private NonFormTimer ds4 = new NonFormTimer();

        public RecordBox(int deviceNum, DS4Windows.DS4ControlSettings controlSettings, bool shift)
        {
            InitializeComponent();

            recordBoxVM = new RecordBoxViewModel(deviceNum, controlSettings, shift);
            mouseButtonsPanel.Visibility = Visibility.Hidden;
            extraConPanel.Visibility = Visibility.Hidden;

            ds4.Elapsed += Ds4_Tick;
            ds4.Interval = 10;
            DataContext = recordBoxVM;
            SetupLateEvents();
        }

        private void Ds4_Tick(object sender, System.Timers.ElapsedEventArgs e)
        {
            recordBoxVM.ProcessDS4Tick();
        }

        private void SetupLateEvents()
        {
            macroListBox.SelectionChanged += MacroListBox_SelectionChanged;
        }

        private void MacroListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!recordBoxVM.Recording)
            {
                if (recordBoxVM.MacroStepIndex >= 0)
                {
                    MacroStepItem item = recordBoxVM.MacroSteps[recordBoxVM.MacroStepIndex];
                    recordBtn.Content = $"Record Before {item.Step.Name}";
                }
                else
                {
                    recordBtn.Content = "Record";
                }
            }
        }

        private void MacroListBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!recordBoxVM.Recording)
            {
                recordBtn.Content = "Record";
                recordBoxVM.EditMacroIndex = -1;
            }
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
                    extraConPanel.Visibility = Visibility.Visible;
                }
                if (macroListBox.IsKeyboardFocused)
                {
                    recordBoxVM.AppendIndex = recordBoxVM.MacroStepIndex;
                }

                ds4.Start();
                Enable_Controls(false);
                recordBoxVM.Sw.Restart();
                this.Focus();
            }
            else
            {
                DS4Windows.Program.rootHub.recordingMacro = false;
                recordBoxVM.AppendIndex = -1;
                ds4.Stop();
                recordBtn.Content = "Record";
                mouseButtonsPanel.Visibility = Visibility.Hidden;
                extraConPanel.Visibility = Visibility.Hidden;
                recordBoxVM.Sw.Stop();

                if (recordBoxVM.Toggle4thMouse)
                {
                    FourMouseBtnAction();
                }
                if (recordBoxVM.Toggle5thMouse)
                {
                    FiveMouseBtnAction();
                }
                if (recordBoxVM.ToggleLightbar)
                {
                    ChangeLightbarAction();
                }
                if (recordBoxVM.ToggleRumble)
                {
                    ChangeRumbleAction();
                }

                Enable_Controls(true);
            }

            recordBoxVM.EditMacroIndex = -1;
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

        private void ChangeLightbarAction()
        {
            bool light = recordBoxVM.ToggleLightbar = !recordBoxVM.ToggleLightbar;
            if (light)
            {
                changeLightBtn.Content = "Reset Lightbar Color";
                DS4Windows.MacroStep step = new DS4Windows.MacroStep(1255255255, $"Lightbar Color: 255,255,255",
                            DS4Windows.MacroStep.StepType.ActDown, DS4Windows.MacroStep.StepOutput.Lightbar);
                recordBoxVM.AddMacroStep(step);
            }
            else
            {
                changeLightBtn.Content = "Change Lightbar Color";
                DS4Windows.MacroStep step = new DS4Windows.MacroStep(1000000000, $"Reset Lightbar",
                            DS4Windows.MacroStep.StepType.ActUp, DS4Windows.MacroStep.StepOutput.Lightbar);
                recordBoxVM.AddMacroStep(step);
            }
        }

        private void ChangeLightBtn_Click(object sender, RoutedEventArgs e)
        {
            ChangeLightbarAction();
        }

        private void ChangeRumbleAction()
        {
            bool rumble = recordBoxVM.ToggleRumble = !recordBoxVM.ToggleRumble;
            if (rumble)
            {
                addRumbleBtn.Content = "Stop Rumble";
                DS4Windows.MacroStep step = new DS4Windows.MacroStep(1255255, $"Rumble 255,255",
                            DS4Windows.MacroStep.StepType.ActDown, DS4Windows.MacroStep.StepOutput.Rumble);
                recordBoxVM.AddMacroStep(step);
            }
            else
            {
                addRumbleBtn.Content = "Add Rumble";
                DS4Windows.MacroStep step = new DS4Windows.MacroStep(1000000, $"Stop Rumble",
                            DS4Windows.MacroStep.StepType.ActUp, DS4Windows.MacroStep.StepOutput.Rumble);
                recordBoxVM.AddMacroStep(step);
            }
        }

        private void AddRumbleBtn_Click(object sender, RoutedEventArgs e)
        {
            ChangeRumbleAction();
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
                recordBoxVM.KeysdownMap.TryGetValue(value, out bool isdown);
                if (!isdown)
                {
                    DS4Windows.MacroStep step = new DS4Windows.MacroStep(KeyInterop.VirtualKeyFromKey(e.Key), e.Key.ToString(),
                            DS4Windows.MacroStep.StepType.ActDown, DS4Windows.MacroStep.StepOutput.Key);
                    recordBoxVM.AddMacroStep(step);
                    recordBoxVM.KeysdownMap.Add(value, true);
                }

                //Console.WriteLine(e.Key);
                //Console.WriteLine(e.SystemKey);
            }
            else if (e.Key == Key.Delete && recordBoxVM.MacroStepIndex >= 0)
            {
                recordBoxVM.MacroSteps.RemoveAt(recordBoxVM.MacroStepIndex);
            }
        }

        private void UserControl_KeyUp(object sender, KeyEventArgs e)
        {
            if (recordBoxVM.Recording)
            {
                int value = KeyInterop.VirtualKeyFromKey(e.Key);
                recordBoxVM.KeysdownMap.TryGetValue(value, out bool isdown);
                if (isdown)
                {
                    DS4Windows.MacroStep step = new DS4Windows.MacroStep(KeyInterop.VirtualKeyFromKey(e.Key), e.Key.ToString(),
                            DS4Windows.MacroStep.StepType.ActUp, DS4Windows.MacroStep.StepOutput.Key);
                    recordBoxVM.AddMacroStep(step);
                    recordBoxVM.KeysdownMap.Remove(value);
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
                    recordBoxVM.EditMacroIndex = recordBoxVM.MacroStepIndex;
                }
                else if (item.Step.OutputType == DS4Windows.MacroStep.StepOutput.Rumble &&
                    item.Step.ActType == DS4Windows.MacroStep.StepType.ActDown)
                {
                    ListBoxItem lbitem = macroListBox.ItemContainerGenerator.ContainerFromIndex(recordBoxVM.MacroStepIndex)
                        as ListBoxItem;
                    lbitem.ContentTemplate = this.FindResource("EditRumbleTemplate") as DataTemplate;
                    recordBoxVM.EditMacroIndex = recordBoxVM.MacroStepIndex;
                }
                else if (item.Step.OutputType == DS4Windows.MacroStep.StepOutput.Lightbar &&
                    item.Step.ActType == DS4Windows.MacroStep.StepType.ActDown)
                {
                    colorDialog = new ColorPickerWindow();
                    colorDialog.Owner = Application.Current.MainWindow;
                    Color tempcolor = item.LightbarColorValue();
                    colorDialog.colorPicker.SelectedColor = tempcolor;
                    recordBoxVM.StartForcedColor(tempcolor);
                    colorDialog.ColorChanged += (sender2, color) =>
                    {
                        recordBoxVM.UpdateForcedColor(color);
                    };
                    colorDialog.ShowDialog();
                    recordBoxVM.EndForcedColor();
                    item.UpdateLightbarValue(colorDialog.colorPicker.SelectedColor.GetValueOrDefault());

                    FocusNavigationDirection focusDirection = FocusNavigationDirection.Next;
                    TraversalRequest request = new TraversalRequest(focusDirection);
                    UIElement elementWithFocus = Keyboard.FocusedElement as UIElement;
                    elementWithFocus?.MoveFocus(request);
                }
            }
        }

        private void EditTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (recordBoxVM.EditMacroIndex >= 0)
            {
                ListBoxItem lbitem = macroListBox.ItemContainerGenerator.ContainerFromIndex(recordBoxVM.MacroStepIndex)
                        as ListBoxItem;
                lbitem.ContentTemplate = this.FindResource("DisplayTemplate") as DataTemplate;
                recordBoxVM.EditMacroIndex = -1;
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

        private void FourMouseBtnAction()
        {
            int value = 259;
            recordBoxVM.KeysdownMap.TryGetValue(value, out bool isdown);
            if (!isdown)
            {
                DS4Windows.MacroStep step = new DS4Windows.MacroStep(value, DS4Windows.MacroParser.macroInputNames[value],
                            DS4Windows.MacroStep.StepType.ActDown, DS4Windows.MacroStep.StepOutput.Button);
                recordBoxVM.AddMacroStep(step);
                recordBoxVM.KeysdownMap.Add(value, true);
            }
            else
            {
                DS4Windows.MacroStep step = new DS4Windows.MacroStep(value, DS4Windows.MacroParser.macroInputNames[value],
                            DS4Windows.MacroStep.StepType.ActUp, DS4Windows.MacroStep.StepOutput.Button);
                recordBoxVM.AddMacroStep(step);
                recordBoxVM.KeysdownMap.Remove(value);
            }
        }

        private void FourMouseBtn_Click(object sender, RoutedEventArgs e)
        {
            FourMouseBtnAction();
        }

        private void FiveMouseBtnAction()
        {
            int value = 260;
            recordBoxVM.KeysdownMap.TryGetValue(value, out bool isdown);
            if (!isdown)
            {
                DS4Windows.MacroStep step = new DS4Windows.MacroStep(value, DS4Windows.MacroParser.macroInputNames[value],
                            DS4Windows.MacroStep.StepType.ActDown, DS4Windows.MacroStep.StepOutput.Button);
                recordBoxVM.AddMacroStep(step);
                recordBoxVM.KeysdownMap.Add(value, true);
            }
            else
            {
                DS4Windows.MacroStep step = new DS4Windows.MacroStep(value, DS4Windows.MacroParser.macroInputNames[value],
                            DS4Windows.MacroStep.StepType.ActUp, DS4Windows.MacroStep.StepOutput.Button);
                recordBoxVM.AddMacroStep(step);
                recordBoxVM.KeysdownMap.Remove(value);
            }
        }

        private void FiveMouseBtn_Click(object sender, RoutedEventArgs e)
        {
            FiveMouseBtnAction();
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
                recordBoxVM.KeysdownMap.Add(value, true);
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
                recordBoxVM.KeysdownMap.Remove(value);
            }
        }

        private void RumbleEditPabel_LostFocus(object sender, RoutedEventArgs e)
        {
            if (recordBoxVM.EditMacroIndex >= 0)
            {
                ListBoxItem lbitem = macroListBox.ItemContainerGenerator.ContainerFromIndex(recordBoxVM.MacroStepIndex)
                        as ListBoxItem;
                lbitem.ContentTemplate = this.FindResource("DisplayTemplate") as DataTemplate;
                recordBoxVM.EditMacroIndex = -1;
            }
        }
    }
}
