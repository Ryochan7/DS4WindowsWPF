using DS4WinWPF.DS4Forms.ViewModel;
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

        private ProfileSettingsViewModel profileSettingsVM;
        private ProfileEntity currentProfile;

        public event EventHandler Closed;

        private Dictionary<Button, ImageBrush> hoverImages =
            new Dictionary<Button, ImageBrush>();
        private Dictionary<Button, HoverImageInfo> hoverLocations = new Dictionary<Button, HoverImageInfo>();

        public ProfileEditor(int device)
        {
            InitializeComponent();

            emptyColorGB.Visibility = Visibility.Collapsed;
            profileSettingsVM = new ProfileSettingsViewModel(device);
            picBoxHover.Visibility = Visibility.Hidden;
            picBoxHover2.Visibility = Visibility.Hidden;

            RemoveHoverBtnText();
            PopulateHoverImages();
            PopulateHoverLocations();
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

            profileSettingsTabCon.DataContext = profileSettingsVM;
            touchpadSettingsPanel.DataContext = profileSettingsVM;
            mappingListBox.DataContext = null;
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            Closed?.Invoke(this, EventArgs.Empty);
        }

        private void CrossConBtn_Click(object sender, RoutedEventArgs e)
        {
            _ = sender as Button;
        }

        private void ContBtn_MouseEnter(object sender, MouseEventArgs e)
        {
            Button control = sender as Button;
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
        }

        private void ContBtn_MouseLeave(object sender, MouseEventArgs e)
        {
            Button control = sender as Button;
            //control.Background = new SolidColorBrush(Colors.Transparent);
            Canvas.SetLeft(picBoxHover, 0);
            Canvas.SetTop(picBoxHover, 0);
            picBoxHover.Visibility = Visibility.Hidden;
        }
    }
}
