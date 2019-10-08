using DS4WinWPF.DS4Forms.ViewModel;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace DS4WinWPF.DS4Forms
{
    /// <summary>
    /// Interaction logic for ProfileEditor.xaml
    /// </summary>
    public partial class ProfileEditor : UserControl
    {
        private class HoverImageInfo
        {
            public Point point;
            public Size size;
        }

        private int deviceNum;
        private ProfileSettingsViewModel profileSettingsVM;
        private MappingListViewModel mappingListVM;
        private ProfileEntity currentProfile;

        public event EventHandler Closed;
        public delegate void CreatedProfileHandler(ProfileEditor sender, string profile);
        public event CreatedProfileHandler CreatedProfile;

        private Dictionary<Button, ImageBrush> hoverImages =
            new Dictionary<Button, ImageBrush>();
        private Dictionary<Button, HoverImageInfo> hoverLocations = new Dictionary<Button, HoverImageInfo>();
        private Dictionary<Button, int> hoverIndexes = new Dictionary<Button, int>();
        private Dictionary<int, Button> reverseHoverIndexes = new Dictionary<int, Button>();

        private StackPanel activeTouchPanel;
        private StackPanel activeGyroModePanel;
        private bool keepsize;
        public bool Keepsize { get => keepsize; }

        public ProfileEditor(int device)
        {
            InitializeComponent();

            deviceNum = device;
            emptyColorGB.Visibility = Visibility.Collapsed;
            profileSettingsVM = new ProfileSettingsViewModel(device);
            picBoxHover.Visibility = Visibility.Hidden;
            picBoxHover2.Visibility = Visibility.Hidden;
            bool touchMouse = profileSettingsVM.UseTouchMouse;
            if (!touchMouse)
            {
                useMousePanel.Visibility = Visibility.Collapsed;
                useControlsPanel.Visibility = Visibility.Visible;
                useTouchControlsRadio.IsChecked = true;
            }
            else
            {
                useTouchMouseRadio.IsChecked = true;
            }
            
            //useControlsPanel.Visibility = !touchMouse ? Visibility.Visible : Visibility.Collapsed;
            activeTouchPanel = touchMouse ? useMousePanel : useControlsPanel;
            //activeTouchPanel = useMousePanel;

            switch (profileSettingsVM.GyroOutModeIndex)
            {
                case 0:
                    activeGyroModePanel = gyroControlsPanel; break;
                case 1:
                    activeGyroModePanel = gyroMousePanel; break;
                case 2:
                    activeGyroModePanel = gyroMouseJoystickPanel; break;
                default:
                    activeGyroModePanel = gyroControlsPanel; break;
            }

            //activeGyroModePanel = gyroControlsPanel;
            gyroControlsPanel.Visibility = Visibility.Collapsed;
            gyroMousePanel.Visibility = Visibility.Collapsed;
            gyroMouseJoystickPanel.Visibility = Visibility.Collapsed;
            activeGyroModePanel.Visibility = Visibility.Visible;

            mappingListVM = new MappingListViewModel(deviceNum, profileSettingsVM.ContType);

            RemoveHoverBtnText();
            PopulateHoverImages();
            PopulateHoverLocations();
            PopulateHoverIndexes();
            PopulateReverseHoverIndexes();

            ColorByBatteryPerCheck();
            AssignTiltAssociation();
            AssignSwipeAssociation();
            SetupEvents();
        }

        private void SetupEvents()
        {
            gyroOutModeCombo.SelectionChanged += GyroOutModeCombo_SelectionChanged;
            outConTypeCombo.SelectionChanged += OutConTypeCombo_SelectionChanged;
            mappingListBox.SelectionChanged += MappingListBox_SelectionChanged;
        }

        private void AssignTiltAssociation()
        {
            gyroZNLb.DataContext = mappingListVM.ControlMap[DS4Windows.DS4Controls.GyroZNeg];
            gyroZPLb.DataContext = mappingListVM.ControlMap[DS4Windows.DS4Controls.GyroZPos];
            gyroXNLb.DataContext = mappingListVM.ControlMap[DS4Windows.DS4Controls.GyroXNeg];
            gyroXLb.DataContext = mappingListVM.ControlMap[DS4Windows.DS4Controls.GyroXPos];
        }

        private void AssignSwipeAssociation()
        {
            swipeUpLb.DataContext = mappingListVM.ControlMap[DS4Windows.DS4Controls.SwipeUp];
            swipeDownLb.DataContext = mappingListVM.ControlMap[DS4Windows.DS4Controls.SwipeDown];
            swipeLeftLb.DataContext = mappingListVM.ControlMap[DS4Windows.DS4Controls.SwipeLeft];
            swipeRightLb.DataContext = mappingListVM.ControlMap[DS4Windows.DS4Controls.SwipeRight];
        }

        private void MappingListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (mappingListVM.SelectedIndex >= 0)
            {
                if (reverseHoverIndexes.TryGetValue(mappingListVM.SelectedIndex, out Button tempBtn))
                {
                    InputControlHighlight(tempBtn);
                }
            //;
            }

        }

        private void PopulateReverseHoverIndexes()
        {
            foreach(KeyValuePair<Button, int> pair in hoverIndexes)
            {
                reverseHoverIndexes.Add(pair.Value, pair.Key);
            }
        }

        private void PopulateHoverIndexes()
        {
            hoverIndexes[crossConBtn] = 0;
            hoverIndexes[circleConBtn] = 1;
            hoverIndexes[squareConBtn] = 2;
            hoverIndexes[triangleConBtn] = 3;
            hoverIndexes[optionsConBtn] = 4;
            hoverIndexes[shareConBtn] = 5;
            hoverIndexes[upConBtn] = 6;
            hoverIndexes[rightConBtn] = 7;
            hoverIndexes[downConBtn] = 8;
            hoverIndexes[leftConBtn] = 9;
            hoverIndexes[guideConBtn] = 10;
            hoverIndexes[l1ConBtn] = 11;
            hoverIndexes[r1ConBtn] = 12;
            hoverIndexes[l2ConBtn] = 13;
            hoverIndexes[r2ConBtn] = 14;
            hoverIndexes[l3ConBtn] = 15;
            hoverIndexes[r3ConBtn] = 16;
            hoverIndexes[leftTouchConBtn] = 17;
            hoverIndexes[rightTouchConBtn] = 18;
            hoverIndexes[multiTouchConBtn] = 19;
            hoverIndexes[topTouchConBtn] = 20;

            hoverIndexes[lsuConBtn] = 21;
            hoverIndexes[lsrConBtn] = 22;
            hoverIndexes[lsdConBtn] = 23;
            hoverIndexes[lslConBtn] = 24;

            hoverIndexes[rsuConBtn] = 25;
            hoverIndexes[rsrConBtn] = 26;
            hoverIndexes[rsdConBtn] = 27;
            hoverIndexes[rslConBtn] = 28;

            hoverIndexes[gyroZNBtn] = 29;
            hoverIndexes[gyroZPBtn] = 30;
            hoverIndexes[gyroXNBtn] = 31;
            hoverIndexes[gyroXPBtn] = 32;

            hoverIndexes[swipeUpBtn] = 33;
            hoverIndexes[swipeDownBtn] = 34;
            hoverIndexes[swipeLeftBtn] = 35;
            hoverIndexes[swipeRightBtn] = 36;
        }

        private void PopulateHoverLocations()
        {
            hoverLocations[crossConBtn] = new HoverImageInfo() { point = new Point(368, 134), size = new Size(30, 30) };
            hoverLocations[circleConBtn] = new HoverImageInfo() { point = new Point(400, 108), size = new Size(30, 30) };
            hoverLocations[squareConBtn] = new HoverImageInfo() { point = new Point(337, 108), size = new Size(30, 30) };
            hoverLocations[triangleConBtn] = new HoverImageInfo() { point = new Point(368, 80), size = new Size(30, 30) };
            hoverLocations[l1ConBtn] = new HoverImageInfo() { point = new Point(89, 22), size = new Size(60, 22) };
            hoverLocations[r1ConBtn] = new HoverImageInfo() { point = new Point(354, 25), size = new Size(60, 22) };
            hoverLocations[l2ConBtn] = new HoverImageInfo() { point = new Point(94, 0), size = new Size(52, 22) };
            hoverLocations[r2ConBtn] = new HoverImageInfo() { point = new Point(355, 0), size = new Size(52, 22) };
            hoverLocations[shareConBtn] = new HoverImageInfo() { point = new Point(156, 68), size = new Size(18, 26) };
            hoverLocations[optionsConBtn] = new HoverImageInfo() { point = new Point(328, 68), size = new Size(18, 26) };
            hoverLocations[guideConBtn] = new HoverImageInfo() { point = new Point(238, 158), size = new Size(22, 22) };

            hoverLocations[leftTouchConBtn] = new HoverImageInfo() { point = new Point(166, 48), size = new Size(154, 108) };
            hoverLocations[multiTouchConBtn] = new HoverImageInfo() { point = new Point(166, 48), size = new Size(172, 108) };
            hoverLocations[rightTouchConBtn] = new HoverImageInfo() { point = new Point(178, 48), size = new Size(162, 108) };
            hoverLocations[topTouchConBtn] = new HoverImageInfo() { point = new Point(170, 6), size = new Size(180, 126) };

            hoverLocations[l3ConBtn] = new HoverImageInfo() { point = new Point(159, 159), size = new Size(48, 48) };
            hoverLocations[lsuConBtn] = new HoverImageInfo() { point = new Point(159, 159), size = new Size(48, 48) };
            hoverLocations[lsrConBtn] = new HoverImageInfo() { point = new Point(159, 159), size = new Size(48, 48) };
            hoverLocations[lsdConBtn] = new HoverImageInfo() { point = new Point(159, 159), size = new Size(48, 48) };
            hoverLocations[lslConBtn] = new HoverImageInfo() { point = new Point(159, 159), size = new Size(48, 48) };

            hoverLocations[r3ConBtn] = new HoverImageInfo() { point = new Point(295, 158), size = new Size(48, 48) };
            hoverLocations[rsuConBtn] = new HoverImageInfo() { point = new Point(295, 158), size = new Size(48, 48) };
            hoverLocations[rsrConBtn] = new HoverImageInfo() { point = new Point(295, 158), size = new Size(48, 48) };
            hoverLocations[rsdConBtn] = new HoverImageInfo() { point = new Point(295, 158), size = new Size(48, 48) };
            hoverLocations[rslConBtn] = new HoverImageInfo() { point = new Point(295, 158), size = new Size(48, 48) };

            hoverLocations[upConBtn] = new HoverImageInfo() { point = new Point(104, 82), size = new Size(26, 38) };
            hoverLocations[rightConBtn] = new HoverImageInfo() { point = new Point(124, 110), size = new Size(38, 26) };
            hoverLocations[downConBtn] = new HoverImageInfo() { point = new Point(104, 124), size = new Size(26, 38) };
            hoverLocations[leftConBtn] = new HoverImageInfo() { point = new Point(72, 110), size = new Size(38, 26) };
        }

        private void RemoveHoverBtnText()
        {
            crossConBtn.Content = "";
            circleConBtn.Content = "";
            squareConBtn.Content = "";
            triangleConBtn.Content = "";
            l1ConBtn.Content = "";
            r1ConBtn.Content = "";
            l2ConBtn.Content = "";
            r2ConBtn.Content = "";
            shareConBtn.Content = "";
            optionsConBtn.Content = "";
            guideConBtn.Content = "";
            leftTouchConBtn.Content = "";
            multiTouchConBtn.Content = "";
            rightTouchConBtn.Content = "";
            topTouchConBtn.Content = "";

            l3ConBtn.Content = "";
            lsuConBtn.Content = "";
            lsrConBtn.Content = "";
            lsdConBtn.Content = "";
            lslConBtn.Content = "";

            r3ConBtn.Content = "";
            rsuConBtn.Content = "";
            rsrConBtn.Content = "";
            rsdConBtn.Content = "";
            rslConBtn.Content = "";

            upConBtn.Content = "";
            rightConBtn.Content = "";
            downConBtn.Content = "";
            leftConBtn.Content = "";
        }

        private void PopulateHoverImages()
        {
            ImageSourceConverter sourceConverter = new ImageSourceConverter();

            ImageSource temp = sourceConverter.
                ConvertFromString("pack://application:,,,/DS4WinWPF;component/Resources/DS4-Config_Cross.png") as ImageSource;
            ImageBrush crossHover = new ImageBrush(temp);

            temp = sourceConverter.
                ConvertFromString("pack://application:,,,/DS4WinWPF;component/Resources/DS4-Config_Circle.png") as ImageSource;
            ImageBrush circleHover = new ImageBrush(temp);

            temp = sourceConverter.
                ConvertFromString("pack://application:,,,/DS4WinWPF;component/Resources/DS4-Config_Square.png") as ImageSource;
            ImageBrush squareHover = new ImageBrush(temp);

            temp = sourceConverter.
                ConvertFromString("pack://application:,,,/DS4WinWPF;component/Resources/DS4-Config_Triangle.png") as ImageSource;
            ImageBrush triangleHover = new ImageBrush(temp);

            temp = sourceConverter.
                ConvertFromString("pack://application:,,,/DS4WinWPF;component/Resources/DS4-Config_L1.png") as ImageSource;
            ImageBrush l1Hover = new ImageBrush(temp);

            temp = sourceConverter.
                ConvertFromString("pack://application:,,,/DS4WinWPF;component/Resources/DS4-Config_R1.png") as ImageSource;
            ImageBrush r1Hover = new ImageBrush(temp);

            temp = sourceConverter.
                ConvertFromString("pack://application:,,,/DS4WinWPF;component/Resources/DS4-Config_L2.png") as ImageSource;
            ImageBrush l2Hover = new ImageBrush(temp);

            temp = sourceConverter.
                ConvertFromString("pack://application:,,,/DS4WinWPF;component/Resources/DS4-Config_R2.png") as ImageSource;
            ImageBrush r2Hover = new ImageBrush(temp);

            temp = sourceConverter.
                ConvertFromString("pack://application:,,,/DS4WinWPF;component/Resources/DS4-Config_Share.png") as ImageSource;
            ImageBrush shareHover = new ImageBrush(temp);

            temp = sourceConverter.
                ConvertFromString("pack://application:,,,/DS4WinWPF;component/Resources/DS4-Config_options.png") as ImageSource;
            ImageBrush optionsHover = new ImageBrush(temp);

            temp = sourceConverter.
                ConvertFromString("pack://application:,,,/DS4WinWPF;component/Resources/DS4-Config_PS.png") as ImageSource;
            ImageBrush guideHover = new ImageBrush(temp);

            temp = sourceConverter.
                ConvertFromString("pack://application:,,,/DS4WinWPF;component/Resources/DS4-Config_TouchLeft.png") as ImageSource;
            ImageBrush leftTouchHover = new ImageBrush(temp);

            temp = sourceConverter.
                ConvertFromString("pack://application:,,,/DS4WinWPF;component/Resources/DS4-Config_TouchMulti.png") as ImageSource;
            ImageBrush multiTouchTouchHover = new ImageBrush(temp);

            temp = sourceConverter.
                ConvertFromString("pack://application:,,,/DS4WinWPF;component/Resources/DS4-Config_TouchRight.png") as ImageSource;
            ImageBrush rightTouchHover = new ImageBrush(temp);

            temp = sourceConverter.
                ConvertFromString("pack://application:,,,/DS4WinWPF;component/Resources/DS4-Config_TouchUpper.png") as ImageSource;
            ImageBrush topTouchHover = new ImageBrush(temp);


            temp = sourceConverter.
                ConvertFromString("pack://application:,,,/DS4WinWPF;component/Resources/DS4-Config_LS.png") as ImageSource;
            ImageBrush l3Hover = new ImageBrush(temp);

            temp = sourceConverter.
                ConvertFromString("pack://application:,,,/DS4WinWPF;component/Resources/DS4-Config_LS.png") as ImageSource;
            ImageBrush lsuHover = new ImageBrush(temp);

            temp = sourceConverter.
                ConvertFromString("pack://application:,,,/DS4WinWPF;component/Resources/DS4-Config_LS.png") as ImageSource;
            ImageBrush lsrHover = new ImageBrush(temp);

            temp = sourceConverter.
                ConvertFromString("pack://application:,,,/DS4WinWPF;component/Resources/DS4-Config_LS.png") as ImageSource;
            ImageBrush lsdHover = new ImageBrush(temp);

            temp = sourceConverter.
                ConvertFromString("pack://application:,,,/DS4WinWPF;component/Resources/DS4-Config_LS.png") as ImageSource;
            ImageBrush lslHover = new ImageBrush(temp);


            temp = sourceConverter.
                ConvertFromString("pack://application:,,,/DS4WinWPF;component/Resources/DS4-Config_RS.png") as ImageSource;
            ImageBrush r3Hover = new ImageBrush(temp);

            temp = sourceConverter.
                ConvertFromString("pack://application:,,,/DS4WinWPF;component/Resources/DS4-Config_RS.png") as ImageSource;
            ImageBrush rsuHover = new ImageBrush(temp);

            temp = sourceConverter.
                ConvertFromString("pack://application:,,,/DS4WinWPF;component/Resources/DS4-Config_RS.png") as ImageSource;
            ImageBrush rsrHover = new ImageBrush(temp);

            temp = sourceConverter.
                ConvertFromString("pack://application:,,,/DS4WinWPF;component/Resources/DS4-Config_RS.png") as ImageSource;
            ImageBrush rsdHover = new ImageBrush(temp);

            temp = sourceConverter.
                ConvertFromString("pack://application:,,,/DS4WinWPF;component/Resources/DS4-Config_RS.png") as ImageSource;
            ImageBrush rslHover = new ImageBrush(temp);


            temp = sourceConverter.
                ConvertFromString("pack://application:,,,/DS4WinWPF;component/Resources/DS4-Config_Up.png") as ImageSource;
            ImageBrush upHover = new ImageBrush(temp);

            temp = sourceConverter.
                ConvertFromString("pack://application:,,,/DS4WinWPF;component/Resources/DS4-Config_Right.png") as ImageSource;
            ImageBrush rightHover = new ImageBrush(temp);

            temp = sourceConverter.
                ConvertFromString("pack://application:,,,/DS4WinWPF;component/Resources/DS4-Config_Down.png") as ImageSource;
            ImageBrush downHover = new ImageBrush(temp);

            temp = sourceConverter.
                ConvertFromString("pack://application:,,,/DS4WinWPF;component/Resources/DS4-Config_Left.png") as ImageSource;
            ImageBrush leftHover = new ImageBrush(temp);

            hoverImages[crossConBtn] = crossHover;
            hoverImages[circleConBtn] = circleHover;
            hoverImages[squareConBtn] = squareHover;
            hoverImages[triangleConBtn] = triangleHover;
            hoverImages[l1ConBtn] = l1Hover;
            hoverImages[r1ConBtn] = r1Hover;
            hoverImages[l2ConBtn] = l2Hover;
            hoverImages[r2ConBtn] = r2Hover;
            hoverImages[shareConBtn] = shareHover;
            hoverImages[optionsConBtn] = optionsHover;
            hoverImages[guideConBtn] = guideHover;

            hoverImages[leftTouchConBtn] = leftTouchHover;
            hoverImages[multiTouchConBtn] = multiTouchTouchHover;
            hoverImages[rightTouchConBtn] = rightTouchHover;
            hoverImages[topTouchConBtn] = topTouchHover;
            hoverImages[l3ConBtn] = l3Hover;
            hoverImages[lsuConBtn] = lsuHover;
            hoverImages[lsrConBtn] = lsrHover;
            hoverImages[lsdConBtn] = lsdHover;
            hoverImages[lslConBtn] = lslHover;
            hoverImages[r3ConBtn] = r3Hover;
            hoverImages[rsuConBtn] = rsuHover;
            hoverImages[rsrConBtn] = rsrHover;
            hoverImages[rsdConBtn] = rsdHover;
            hoverImages[rslConBtn] = rslHover;

            hoverImages[upConBtn] = upHover;
            hoverImages[rightConBtn] = rightHover;
            hoverImages[downConBtn] = downHover;
            hoverImages[leftConBtn] = leftHover;
        }

        public void Reload(int device, ProfileEntity profile = null)
        {
            profileSettingsTabCon.DataContext = null;
            touchpadSettingsPanel.DataContext = null;
            mappingListBox.DataContext = null;

            deviceNum = device;
            if (profile != null)
            {
                currentProfile = profile;
                if (device == 4)
                {
                    DS4Windows.Global.ProfilePath[4] = profile.Name;
                }

                DS4Windows.Global.LoadProfile(device, false, App.rootHub);
                profileNameTxt.Text = profile.Name;
            }
            else
            {
                currentProfile = null;
            }

            mappingListVM.UpdateMappings();
            profileSettingsVM.PopulateTouchDisInver(touchDisInvertBtn.ContextMenu);
            profileSettingsVM.PopulateGyroMouseTrig(gyroMouseTrigBtn.ContextMenu);
            profileSettingsVM.PopulateGyroMouseStickTrig(gyroMouseStickTrigBtn.ContextMenu);
            profileSettingsTabCon.DataContext = profileSettingsVM;
            touchpadSettingsPanel.DataContext = profileSettingsVM;
            mappingListBox.DataContext = mappingListVM;
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            if (deviceNum < 4)
            {
                App.rootHub.setRumble(0, 0, deviceNum);
            }
            DS4Windows.Global.LoadProfile(deviceNum, false, App.rootHub);
            Closed?.Invoke(this, EventArgs.Empty);
        }

        private void CrossConBtn_Click(object sender, RoutedEventArgs e)
        {
            _ = sender as Button;
            BindingWindow window = new BindingWindow(deviceNum, mappingListVM.Mappings[mappingListVM.SelectedIndex]);
            window.Owner = App.Current.MainWindow;
            window.ShowDialog();
        }

        private void InputControlHighlight(Button control)
        {
            if (hoverImages.TryGetValue(control, out ImageBrush tempBrush))
            {
                picBoxHover.Source = tempBrush.ImageSource;
                //picBoxHover.Width = tempBrush.ImageSource.Width * .8;
                //picBoxHover.Height = tempBrush.ImageSource.Height * .8;
                //control.Background = tempBrush;
                //control.Background = new SolidColorBrush(Colors.Green);
                //control.Width = tempBrush.ImageSource.Width;
                //control.Height = tempBrush.ImageSource.Height;
            }

            if (hoverLocations.TryGetValue(control, out HoverImageInfo tempInfo))
            {
                Canvas.SetLeft(picBoxHover, tempInfo.point.X);
                Canvas.SetTop(picBoxHover, tempInfo.point.Y);
                picBoxHover.Width = tempInfo.size.Width;
                picBoxHover.Height = tempInfo.size.Height;
                //picBoxHover.Stretch = Stretch.Fill;
                picBoxHover.Visibility = Visibility.Visible;
            }

            if (hoverIndexes.TryGetValue(control, out int tempIndex))
            {
                mappingListVM.SelectedIndex = tempIndex;
                mappingListBox.ScrollIntoView(mappingListBox.SelectedItem);
                MappedControl mapped = mappingListVM.Mappings[tempIndex];
                string display = $"{mapped.ControlName}: {mapped.MappingName}";
                if (mapped.HasShiftAction())
                {
                    display += "\n Shift: ";
                    display += mapped.ShiftMappingName;
                }

                highlightControlDisplayLb.Content = display;
            }
        }

        private void ContBtn_MouseEnter(object sender, MouseEventArgs e)
        {
            Button control = sender as Button;
            InputControlHighlight(control);
        }

        private void ContBtn_MouseLeave(object sender, MouseEventArgs e)
        {
            Button control = sender as Button;
            //control.Background = new SolidColorBrush(Colors.Transparent);
            Canvas.SetLeft(picBoxHover, 0);
            Canvas.SetTop(picBoxHover, 0);
            picBoxHover.Visibility = Visibility.Hidden;
        }

        private void UseTouchMouseRadio_Click(object sender, RoutedEventArgs e)
        {
            activeTouchPanel.Visibility = Visibility.Collapsed;
            useMousePanel.Visibility = Visibility.Visible;
            activeTouchPanel = useMousePanel;
            profileSettingsVM.UseTouchMouse = true;
        }

        private void UseTouchControlsRadio_Click(object sender, RoutedEventArgs e)
        {
            activeTouchPanel.Visibility = Visibility.Collapsed;
            useControlsPanel.Visibility = Visibility.Visible;
            activeTouchPanel = useControlsPanel;
            profileSettingsVM.UseTouchMouse = false;
        }

        private void GyroOutModeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int idx = gyroOutModeCombo.SelectedIndex;
            if (idx >= 0)
            {
                activeGyroModePanel.Visibility = Visibility.Collapsed;

                if (idx == 0)
                {
                    activeGyroModePanel = gyroControlsPanel;
                }
                else if (idx == 1)
                {
                    activeGyroModePanel = gyroMousePanel;
                }
                else if (idx == 2)
                {
                    activeGyroModePanel = gyroMouseJoystickPanel;
                }

                activeGyroModePanel.Visibility = Visibility.Visible;

                if (deviceNum < 4)
                {
                    App.rootHub.touchPad[deviceNum]?.ResetToggleGyroM();
                }
            }
        }

        private void SetLateProperties()
        {
            DS4Windows.Global.BTPollRate[deviceNum] = profileSettingsVM.TempBTPollRateIndex;
            DS4Windows.Global.OutContType[deviceNum] = profileSettingsVM.TempConType;
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (deviceNum < 4)
            {
                App.rootHub.setRumble(0, 0, deviceNum);
            }

            string temp = profileNameTxt.Text;
            if (!string.IsNullOrWhiteSpace(temp) &&
                temp.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) == -1)
            {
                SetLateProperties();
                DS4Windows.Global.ProfilePath[deviceNum] =
                    DS4Windows.Global.OlderProfilePath[deviceNum] = temp;

                if (currentProfile != null)
                {
                    if (temp != currentProfile.Name)
                    {
                        //File.Delete(DS4Windows.Global.appdatapath + @"\Profiles\" + currentProfile.Name + ".xml");
                        currentProfile.DeleteFile();
                        currentProfile.Name = temp;
                    }
                }

                if (currentProfile != null)
                {
                    currentProfile.SaveProfile(deviceNum);
                    currentProfile.FireSaved();
                }
                else
                {
                    DS4Windows.Global.SaveProfile(deviceNum, temp);
                    DS4Windows.Global.cacheProfileCustomsFlags(deviceNum);
                    CreatedProfile?.Invoke(this, temp);
                }

                Closed?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                MessageBox.Show(Properties.Resources.ValidName, Properties.Resources.NotValid,
                    MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        private void KeepSizeBtn_Click(object sender, RoutedEventArgs e)
        {
            keepsize = true;
            ImageSourceConverter c = new ImageSourceConverter();
            sizeImage.Source = c.ConvertFromString("pack://application:,,,/DS4WinWPF;component/Resources/checked.png") as ImageSource;
        }

        public void Close()
        {
            if (deviceNum < 4)
            {
                App.rootHub.setRumble(0, 0, deviceNum);
            }

            Closed?.Invoke(this, EventArgs.Empty);
        }

        private void ColorByBatteryPerCk_Click(object sender, RoutedEventArgs e)
        {
            ColorByBatteryPerCheck();
        }

        private void ColorByBatteryPerCheck()
        {
            bool state = profileSettingsVM.ColorBatteryPercent;
            if (state)
            {
                colorGB.Header = "Full";
                emptyColorGB.Visibility = Visibility.Visible;
            }
            else
            {
                colorGB.Header = "Color";
                emptyColorGB.Visibility = Visibility.Hidden;
            }
        }

        private void FlashColorBtn_Click(object sender, RoutedEventArgs e)
        {
            ColorPickerWindow dialog = new ColorPickerWindow();
            dialog.Owner = Application.Current.MainWindow;
            Color tempcolor = profileSettingsVM.FlashColorMedia;
            dialog.colorPicker.SelectedColor = tempcolor;
            profileSettingsVM.StartForcedColor(tempcolor);
            dialog.ColorChanged += (sender2, color) =>
            {
                profileSettingsVM.UpdateForcedColor(color);
            };
            dialog.ShowDialog();
            profileSettingsVM.CopyForceFlashColor();
            profileSettingsVM.EndForcedColor();
        }

        private void LowColorBtn_Click(object sender, RoutedEventArgs e)
        {
            ColorPickerWindow dialog = new ColorPickerWindow();
            dialog.Owner = Application.Current.MainWindow;
            Color tempcolor = profileSettingsVM.LowColorMedia;
            dialog.colorPicker.SelectedColor = tempcolor;
            profileSettingsVM.StartForcedColor(tempcolor);
            dialog.ColorChanged += (sender2, color) =>
            {
                profileSettingsVM.UpdateForcedColor(color);
            };
            dialog.ShowDialog();
            profileSettingsVM.CopyForceLowColor();
            profileSettingsVM.EndForcedColor();
        }

        private void HeavyRumbleTestBtn_Click(object sender, RoutedEventArgs e)
        {
            if (deviceNum < 4)
            {
                DS4Windows.DS4Device d = App.rootHub.DS4Controllers[deviceNum];
                if (d != null)
                {
                    bool rumbleActive = profileSettingsVM.HeavyRumbleActive;
                    if (!rumbleActive)
                    {
                        profileSettingsVM.HeavyRumbleActive = true;
                        d.setRumble(d.LeftHeavySlowRumble,
                            (byte)Math.Min(255, 255 * profileSettingsVM.RumbleBoost / 100));
                        heavyRumbleTestBtn.Content = Properties.Resources.StopHText;
                    }
                    else
                    {
                        profileSettingsVM.HeavyRumbleActive = false;
                        d.setRumble(0, 0);
                        heavyRumbleTestBtn.Content = Properties.Resources.TestHText;
                    }
                }
            }
        }

        private void LightRumbleTestBtn_Click(object sender, RoutedEventArgs e)
        {
            if (deviceNum < 4)
            {
                DS4Windows.DS4Device d = App.rootHub.DS4Controllers[deviceNum];
                if (d != null)
                {
                    bool rumbleActive = profileSettingsVM.LightRumbleActive;
                    if (!rumbleActive)
                    {
                        profileSettingsVM.LightRumbleActive = true;
                        d.setRumble((byte)Math.Min(255, 255 * profileSettingsVM.RumbleBoost / 100),
                            d.RightLightFastRumble);
                        lightRumbleTestBtn.Content = Properties.Resources.StopLText;
                    }
                    else
                    {
                        profileSettingsVM.LightRumbleActive = false;
                        d.setRumble(0, 0);
                        lightRumbleTestBtn.Content = Properties.Resources.TestLText;
                    }
                }
            }
        }

        private void CustomEditorBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void LaunchProgBrowseBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = false;
            dialog.AddExtension = true;
            dialog.DefaultExt = ".exe";
            dialog.Filter = "Exe (*.exe)|*.exe";
            dialog.Title = "Select Program";

            dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            if (dialog.ShowDialog() == true)
            {
                profileSettingsVM.UpdateLaunchProgram(dialog.FileName);
            }
        }

        private void FrictionUD_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (deviceNum < 4)
            {
                App.rootHub.touchPad[deviceNum]?.ResetTrackAccel(frictionUD.Value.GetValueOrDefault());
            }
        }

        private void RainbowBtn_Click(object sender, RoutedEventArgs e)
        {
            bool active = profileSettingsVM.Rainbow != 0.0;
            if (active)
            {
                profileSettingsVM.Rainbow = 0.0;
                colorByBatteryPerCk.Content = Properties.Resources.ColorByBattery;
                colorGB.IsEnabled = true;
                emptyColorGB.IsEnabled = true;
            }
            else
            {
                profileSettingsVM.Rainbow = 5.0;
                colorByBatteryPerCk.Content = Properties.Resources.DimByBattery;
                colorGB.IsEnabled = false;
                emptyColorGB.IsEnabled = false;
            }
        }

        private void ChargingColorBtn_Click(object sender, RoutedEventArgs e)
        {
            ColorPickerWindow dialog = new ColorPickerWindow();
            dialog.Owner = Application.Current.MainWindow;
            Color tempcolor = profileSettingsVM.ChargingColorMedia;
            dialog.colorPicker.SelectedColor = tempcolor;
            profileSettingsVM.StartForcedColor(tempcolor);
            dialog.ColorChanged += (sender2, color) =>
            {
                profileSettingsVM.UpdateForcedColor(color);
            };
            dialog.ShowDialog();
            profileSettingsVM.CopyForceChargingColor();
            profileSettingsVM.EndForcedColor();
        }

        private void SteeringWheelEmulationCalibrateBtn_Click(object sender, RoutedEventArgs e)
        {
            if (profileSettingsVM.SASteeringWheelEmulationAxisIndex > 0)
            {
                DS4Windows.DS4Device d = App.rootHub.DS4Controllers[deviceNum];
                if (d != null)
                {
                    System.Drawing.Point origWheelCenterPoint = new System.Drawing.Point(d.wheelCenterPoint.X, d.wheelCenterPoint.Y);
                    System.Drawing.Point origWheel90DegPointLeft = new System.Drawing.Point(d.wheel90DegPointLeft.X, d.wheel90DegPointLeft.Y);
                    System.Drawing.Point origWheel90DegPointRight = new System.Drawing.Point(d.wheel90DegPointRight.X, d.wheel90DegPointRight.Y);

                    d.WheelRecalibrateActiveState = 1;

                    MessageBoxResult result = MessageBox.Show($"{Properties.Resources.SASteeringWheelEmulationCalibrate}.\n\n" +
                            $"{Properties.Resources.SASteeringWheelEmulationCalibrateInstruction1}.\n" +
                            $"{Properties.Resources.SASteeringWheelEmulationCalibrateInstruction2}.\n" +
                            $"{Properties.Resources.SASteeringWheelEmulationCalibrateInstruction3}.\n\n" +
                            $"{Properties.Resources.SASteeringWheelEmulationCalibrateInstruction}.\n",
                        Properties.Resources.SASteeringWheelEmulationCalibrate, MessageBoxButton.OKCancel, MessageBoxImage.Information, MessageBoxResult.OK);

                    if (result == MessageBoxResult.OK)
                    {
                        // Accept new calibration values (State 3 is "Complete calibration" state)
                        d.WheelRecalibrateActiveState = 3;
                    }
                    else
                    {
                        // Cancel calibration and reset back to original calibration values
                        d.WheelRecalibrateActiveState = 4;

                        d.wheelFullTurnCount = 0;
                        d.wheelCenterPoint = origWheelCenterPoint;
                        d.wheel90DegPointLeft = origWheel90DegPointLeft;
                        d.wheel90DegPointRight = origWheel90DegPointRight;
                    }
                }
                else
                {
                    MessageBox.Show($"{Properties.Resources.SASteeringWheelEmulationCalibrateNoControllerError}.");
                }
            }
        }

        private void TouchDisInvertBtn_Click(object sender, RoutedEventArgs e)
        {
            touchDisInvertBtn.ContextMenu.IsOpen = true;
        }

        private void TouchDisInvertMenuItem_Click(object sender, RoutedEventArgs e)
        {
            profileSettingsVM.UpdateTouchDisInvert(touchDisInvertBtn.ContextMenu);
        }

        private void GyroMouseTrigMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ContextMenu menu = gyroMouseTrigBtn.ContextMenu;
            int itemCount = menu.Items.Count;
            MenuItem alwaysOnItem = menu.Items[itemCount - 1] as MenuItem;

            profileSettingsVM.UpdateGyroMouseTrig(menu, e.OriginalSource == alwaysOnItem);
        }

        private void GyroMouseStickTrigMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ContextMenu menu = gyroMouseStickTrigBtn.ContextMenu;
            int itemCount = menu.Items.Count;
            MenuItem alwaysOnItem = menu.Items[itemCount - 1] as MenuItem;

            profileSettingsVM.UpdateGyroMouseStickTrig(menu, e.OriginalSource == alwaysOnItem);
        }

        private void GyroMouseTrigBtn_Click(object sender, RoutedEventArgs e)
        {
            gyroMouseTrigBtn.ContextMenu.IsOpen = true;
        }

        private void GyroMouseStickTrigBtn_Click(object sender, RoutedEventArgs e)
        {
            gyroMouseStickTrigBtn.ContextMenu.IsOpen = true;
        }

        private void OutConTypeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = outConTypeCombo.SelectedIndex;
            if (index >= 0)
            {
                mappingListVM.UpdateMappingDevType(profileSettingsVM.TempConType);
            }
        }
    }
}
