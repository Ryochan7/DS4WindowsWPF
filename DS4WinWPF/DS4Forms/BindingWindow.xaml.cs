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
using System.Windows.Shapes;
using DS4WinWPF.DS4Forms.ViewModel;

namespace DS4WinWPF.DS4Forms
{
    /// <summary>
    /// Interaction logic for BindingWindow.xaml
    /// </summary>
    public partial class BindingWindow : Window
    {
        private Dictionary<Button, OutBinding> associatedBindings =
            new Dictionary<Button, OutBinding>();
        private Dictionary<int, Button> keyBtnMap = new Dictionary<int, Button>();
        private Dictionary<DS4Windows.X360Controls, Button> conBtnMap =
            new Dictionary<DS4Windows.X360Controls, Button>();
        private Dictionary<DS4Windows.X360Controls, Button> mouseBtnMap =
            new Dictionary<DS4Windows.X360Controls, Button>();
        private BindingWindowViewModel bindingVM;
        private Button highlightBtn;

        public BindingWindow(int deviceNum, MappedControl mappedControl)
        {
            InitializeComponent();

            bindingVM = new BindingWindowViewModel(deviceNum, mappedControl);

            guideBtn.Content = "";
            highlightImg.Visibility = Visibility.Hidden;
            highlightLb.Visibility = Visibility.Hidden;

            InitButtonBindings();
            InitKeyBindings();
            InitInfoMaps();
            FindCurrentHighlightButton();
        }

        private void OutConBtn_MouseEnter(object sender, MouseEventArgs e)
        {
            Button button = sender as Button;
            string name = button.Tag.ToString();
            highlightLb.Content = name;

            double left = Canvas.GetLeft(button);
            double top = Canvas.GetTop(button);

            Canvas.SetLeft(highlightImg, left + (button.Width / 2.0) - (highlightImg.Height / 2.0));
            Canvas.SetTop(highlightImg, top + (button.Height / 2.0) - (highlightImg.Height / 2.0));

            Canvas.SetLeft(highlightLb, left + (button.Width / 2.0) - (highlightLb.ActualWidth / 2.0));
            Canvas.SetTop(highlightLb, top - 30);

            highlightImg.Visibility = Visibility.Visible;
            highlightLb.Visibility = Visibility.Visible;
        }

        private void OutConBtn_MouseLeave(object sender, MouseEventArgs e)
        {
            highlightImg.Visibility = Visibility.Hidden;
            highlightLb.Visibility = Visibility.Hidden;
        }

        private void OutputBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ChangeForCurrentAction()
        {

        }

        private void FindCurrentHighlightButton()
        {
            if (highlightBtn != null)
            {
                highlightBtn.Background = SystemColors.ControlBrush;
            }

            OutBinding binding = bindingVM.CurrentOutBind;
            if (binding.outputType == OutBinding.OutType.Default)
            {
                DS4Windows.X360Controls defaultBind = DS4Windows.Global.defaultButtonMapping[(int)binding.input];
                if (!OutBinding.IsMouseRange(defaultBind))
                {
                    if (conBtnMap.TryGetValue(defaultBind, out Button tempBtn))
                    {
                        OutConBtn_MouseEnter(tempBtn, null);
                        //tempBtn.Background = new SolidColorBrush(Colors.LimeGreen);
                    }
                }
                else
                {
                    if (mouseBtnMap.TryGetValue(binding.control, out Button tempBtn))
                    {
                        tempBtn.Background = new SolidColorBrush(Colors.LimeGreen);
                        highlightBtn = tempBtn;
                    }
                }
            }
            else if (binding.outputType == OutBinding.OutType.Button)
            {
                if (!binding.IsMouse())
                {
                    if (conBtnMap.TryGetValue(binding.control, out Button tempBtn))
                    {
                        OutConBtn_MouseEnter(tempBtn, null);
                        //tempBtn.Background = new SolidColorBrush(Colors.LimeGreen);
                    }
                }
                else
                {
                    if (mouseBtnMap.TryGetValue(binding.control, out Button tempBtn))
                    {
                        tempBtn.Background = new SolidColorBrush(Colors.LimeGreen);
                        highlightBtn = tempBtn;
                    }
                }
            }
            else if (binding.outputType == OutBinding.OutType.Key)
            {
                if (keyBtnMap.TryGetValue(binding.outkey, out Button tempBtn))
                {
                    tempBtn.Background = new SolidColorBrush(Colors.LimeGreen);
                    highlightBtn = tempBtn;
                }
            }
        }

        private void InitInfoMaps()
        {
            foreach(KeyValuePair<Button, OutBinding> pair in associatedBindings)
            {
                Button button = pair.Key;
                OutBinding binding = pair.Value;
                if (binding.outputType == OutBinding.OutType.Button)
                {
                    if (!binding.IsMouse())
                    {
                        conBtnMap.Add(binding.control, button);
                    }
                    else
                    {
                        mouseBtnMap.Add(binding.control, button);
                    }
                }
                else if (binding.outputType == OutBinding.OutType.Key)
                {
                    keyBtnMap.Add(binding.outkey, button);
                }
            }
        }

        private void InitButtonBindings()
        {
            associatedBindings.Add(aBtn,
                new OutBinding() { outputType = OutBinding.OutType.Button, control = DS4Windows.X360Controls.A });
            aBtn.Click += OutputBtn_Click;
            associatedBindings.Add(bBtn,
                new OutBinding() { outputType = OutBinding.OutType.Button, control = DS4Windows.X360Controls.B });
            bBtn.Click += OutputBtn_Click;
            associatedBindings.Add(xBtn,
                new OutBinding() { outputType = OutBinding.OutType.Button, control = DS4Windows.X360Controls.X });
            xBtn.Click += OutputBtn_Click;
            associatedBindings.Add(yBtn,
                new OutBinding() { outputType = OutBinding.OutType.Button, control = DS4Windows.X360Controls.Y });
            yBtn.Click += OutputBtn_Click;
            associatedBindings.Add(lbBtn,
                new OutBinding() { outputType = OutBinding.OutType.Button, control = DS4Windows.X360Controls.LB });
            lbBtn.Click += OutputBtn_Click;
            associatedBindings.Add(ltBtn,
                new OutBinding() { outputType = OutBinding.OutType.Button, control = DS4Windows.X360Controls.LT });
            ltBtn.Click += OutputBtn_Click;
            associatedBindings.Add(rbBtn,
                new OutBinding() { outputType = OutBinding.OutType.Button, control = DS4Windows.X360Controls.RB });
            rbBtn.Click += OutputBtn_Click;
            associatedBindings.Add(rtBtn,
                new OutBinding() { outputType = OutBinding.OutType.Button, control = DS4Windows.X360Controls.RT });
            rtBtn.Click += OutputBtn_Click;
            associatedBindings.Add(backBtn,
                new OutBinding() { outputType = OutBinding.OutType.Button, control = DS4Windows.X360Controls.Back });
            backBtn.Click += OutputBtn_Click;
            associatedBindings.Add(startBtn,
                new OutBinding() { outputType = OutBinding.OutType.Button, control = DS4Windows.X360Controls.Start });
            startBtn.Click += OutputBtn_Click;
            associatedBindings.Add(guideBtn,
                new OutBinding() { outputType = OutBinding.OutType.Button, control = DS4Windows.X360Controls.Guide });
            guideBtn.Click += OutputBtn_Click;
            associatedBindings.Add(lsbBtn,
                new OutBinding() { outputType = OutBinding.OutType.Button, control = DS4Windows.X360Controls.LS });
            lsbBtn.Click += OutputBtn_Click;
            associatedBindings.Add(lsuBtn,
                new OutBinding() { outputType = OutBinding.OutType.Button, control = DS4Windows.X360Controls.LYNeg });
            lsuBtn.Click += OutputBtn_Click;
            associatedBindings.Add(lsrBtn,
                new OutBinding() { outputType = OutBinding.OutType.Button, control = DS4Windows.X360Controls.LXPos });
            lsrBtn.Click += OutputBtn_Click;
            associatedBindings.Add(lsdBtn,
                new OutBinding() { outputType = OutBinding.OutType.Button, control = DS4Windows.X360Controls.LYPos });
            lsdBtn.Click += OutputBtn_Click;
            associatedBindings.Add(lslBtn,
                new OutBinding() { outputType = OutBinding.OutType.Button, control = DS4Windows.X360Controls.LXNeg });
            lslBtn.Click += OutputBtn_Click;
            associatedBindings.Add(dpadUBtn,
                new OutBinding() { outputType = OutBinding.OutType.Button, control = DS4Windows.X360Controls.DpadUp });
            dpadUBtn.Click += OutputBtn_Click;
            associatedBindings.Add(dpadRBtn,
                new OutBinding() { outputType = OutBinding.OutType.Button, control = DS4Windows.X360Controls.DpadRight });
            dpadRBtn.Click += OutputBtn_Click;
            associatedBindings.Add(dpadDBtn,
                new OutBinding() { outputType = OutBinding.OutType.Button, control = DS4Windows.X360Controls.DpadDown });
            dpadDBtn.Click += OutputBtn_Click;
            associatedBindings.Add(dpadLBtn,
                new OutBinding() { outputType = OutBinding.OutType.Button, control = DS4Windows.X360Controls.DpadLeft });
            dpadLBtn.Click += OutputBtn_Click;
            associatedBindings.Add(rsbBtn,
                new OutBinding() { outputType = OutBinding.OutType.Button, control = DS4Windows.X360Controls.RS });
            rsbBtn.Click += OutputBtn_Click;
            associatedBindings.Add(rsuBtn,
                new OutBinding() { outputType = OutBinding.OutType.Button, control = DS4Windows.X360Controls.RYNeg });
            rsuBtn.Click += OutputBtn_Click;
            associatedBindings.Add(rsrBtn,
                new OutBinding() { outputType = OutBinding.OutType.Button, control = DS4Windows.X360Controls.RXPos });
            rsrBtn.Click += OutputBtn_Click;
            associatedBindings.Add(rsdBtn,
                new OutBinding() { outputType = OutBinding.OutType.Button, control = DS4Windows.X360Controls.RYPos });
            rsdBtn.Click += OutputBtn_Click;
            associatedBindings.Add(rslBtn,
                new OutBinding() { outputType = OutBinding.OutType.Button, control = DS4Windows.X360Controls.RXNeg });
            rslBtn.Click += OutputBtn_Click;

            associatedBindings.Add(mouseUpBtn,
                new OutBinding() { outputType = OutBinding.OutType.Button, control = DS4Windows.X360Controls.MouseUp });
            mouseUpBtn.Click += OutputBtn_Click;
            associatedBindings.Add(mouseDownBtn,
                new OutBinding() { outputType = OutBinding.OutType.Button, control = DS4Windows.X360Controls.MouseDown });
            mouseDownBtn.Click += OutputBtn_Click;
            associatedBindings.Add(mouseLeftBtn,
                new OutBinding() { outputType = OutBinding.OutType.Button, control = DS4Windows.X360Controls.MouseLeft });
            mouseLeftBtn.Click += OutputBtn_Click;
            associatedBindings.Add(mouseRightBtn,
                new OutBinding() { outputType = OutBinding.OutType.Button, control = DS4Windows.X360Controls.MouseRight });
            mouseRightBtn.Click += OutputBtn_Click;
            associatedBindings.Add(mouseLBBtn,
                new OutBinding() { outputType = OutBinding.OutType.Button, control = DS4Windows.X360Controls.LeftMouse });
            mouseLBBtn.Click += OutputBtn_Click;
            associatedBindings.Add(mouseMBBtn,
                new OutBinding() { outputType = OutBinding.OutType.Button, control = DS4Windows.X360Controls.MiddleMouse });
            mouseMBBtn.Click += OutputBtn_Click;
            associatedBindings.Add(mouseRBBtn,
                new OutBinding() { outputType = OutBinding.OutType.Button, control = DS4Windows.X360Controls.RightMouse });
            mouseRBBtn.Click += OutputBtn_Click;
            associatedBindings.Add(mouseWheelUBtn,
                new OutBinding() { outputType = OutBinding.OutType.Button, control = DS4Windows.X360Controls.WUP });
            mouseWheelUBtn.Click += OutputBtn_Click;
            associatedBindings.Add(mouseWheelDBtn,
                new OutBinding() { outputType = OutBinding.OutType.Button, control = DS4Windows.X360Controls.WDOWN });
            mouseWheelDBtn.Click += OutputBtn_Click;
        }

        private void InitKeyBindings()
        {
            associatedBindings.Add(escBtn,
                new OutBinding() { outputType = OutBinding.OutType.Key, outkey = 0x1B });
            escBtn.Click += OutputBtn_Click;
            associatedBindings.Add(f1Btn,
                new OutBinding() { outputType = OutBinding.OutType.Key, outkey = 0x70 });
            f1Btn.Click += OutputBtn_Click;
            associatedBindings.Add(f2Btn,
                new OutBinding() { outputType = OutBinding.OutType.Key, outkey = 0x71 });
            f2Btn.Click += OutputBtn_Click;
            associatedBindings.Add(f3Btn,
                new OutBinding() { outputType = OutBinding.OutType.Key, outkey = 0x72 });
            f3Btn.Click += OutputBtn_Click;
            associatedBindings.Add(f4Btn,
                new OutBinding() { outputType = OutBinding.OutType.Key, outkey = 0x73 });
            f4Btn.Click += OutputBtn_Click;
            associatedBindings.Add(f5Btn,
                new OutBinding() { outputType = OutBinding.OutType.Key, outkey = 0x74 });
            f5Btn.Click += OutputBtn_Click;
            associatedBindings.Add(f6Btn,
                new OutBinding() { outputType = OutBinding.OutType.Key, outkey = 0x75 });
            f6Btn.Click += OutputBtn_Click;
            associatedBindings.Add(f7Btn,
                new OutBinding() { outputType = OutBinding.OutType.Key, outkey = 0x76 });
            f7Btn.Click += OutputBtn_Click;
            associatedBindings.Add(f8Btn,
                new OutBinding() { outputType = OutBinding.OutType.Key, outkey = 0x77 });
            f8Btn.Click += OutputBtn_Click;
            associatedBindings.Add(f9Btn,
                new OutBinding() { outputType = OutBinding.OutType.Key, outkey = 0x78 });
            f9Btn.Click += OutputBtn_Click;
            associatedBindings.Add(f10Btn,
                new OutBinding() { outputType = OutBinding.OutType.Key, outkey = 0x79 });
            f10Btn.Click += OutputBtn_Click;
            associatedBindings.Add(f11Btn,
                new OutBinding() { outputType = OutBinding.OutType.Key, outkey = 0x7A });
            f11Btn.Click += OutputBtn_Click;
            associatedBindings.Add(f12Btn,
                new OutBinding() { outputType = OutBinding.OutType.Key, outkey = 0x7B });
            f12Btn.Click += OutputBtn_Click;

            associatedBindings.Add(oem3Btn,
                new OutBinding() { outputType = OutBinding.OutType.Key, outkey = 0xC0 });
            oem3Btn.Click += OutputBtn_Click;
            associatedBindings.Add(oneBtn,
                new OutBinding() { outputType = OutBinding.OutType.Key, outkey = 0x31 });
            oneBtn.Click += OutputBtn_Click;
            associatedBindings.Add(twoBtn,
                new OutBinding() { outputType = OutBinding.OutType.Key, outkey = 0x32 });
            twoBtn.Click += OutputBtn_Click;
            associatedBindings.Add(threeBtn,
                new OutBinding() { outputType = OutBinding.OutType.Key, outkey = 0x33 });
            threeBtn.Click += OutputBtn_Click;
            associatedBindings.Add(fourBtn,
                new OutBinding() { outputType = OutBinding.OutType.Key, outkey = 0x34 });
            fourBtn.Click += OutputBtn_Click;
            associatedBindings.Add(fiveBtn,
                new OutBinding() { outputType = OutBinding.OutType.Key, outkey = 0x35 });
            fiveBtn.Click += OutputBtn_Click;
            associatedBindings.Add(sixBtn,
                new OutBinding() { outputType = OutBinding.OutType.Key, outkey = 0x36 });
            sixBtn.Click += OutputBtn_Click;
            associatedBindings.Add(sevenBtn,
                new OutBinding() { outputType = OutBinding.OutType.Key, outkey = 0x37 });
            sevenBtn.Click += OutputBtn_Click;
            associatedBindings.Add(eightBtn,
                new OutBinding() { outputType = OutBinding.OutType.Key, outkey = 0x38 });
            eightBtn.Click += OutputBtn_Click;
            associatedBindings.Add(nineBtn,
                new OutBinding() { outputType = OutBinding.OutType.Key, outkey = 0x39 });
            nineBtn.Click += OutputBtn_Click;
            associatedBindings.Add(zeroBtn,
                new OutBinding() { outputType = OutBinding.OutType.Key, outkey = 0x30 });
            zeroBtn.Click += OutputBtn_Click;
            associatedBindings.Add(minusBtn,
                new OutBinding() { outputType = OutBinding.OutType.Key, outkey = 0xBD });
            minusBtn.Click += OutputBtn_Click;
            associatedBindings.Add(equalBtn,
                new OutBinding() { outputType = OutBinding.OutType.Key, outkey = 0xBB });
            equalBtn.Click += OutputBtn_Click;
            associatedBindings.Add(bsBtn,
                new OutBinding() { outputType = OutBinding.OutType.Key, outkey = 0x08 });
            bsBtn.Click += OutputBtn_Click;

            associatedBindings.Add(tabBtn,
                new OutBinding() { outputType = OutBinding.OutType.Key, outkey = 0x09 });
            tabBtn.Click += OutputBtn_Click;
            associatedBindings.Add(qKey,
                new OutBinding() { outputType = OutBinding.OutType.Key, outkey = 0x51 });
            qKey.Click += OutputBtn_Click;
            associatedBindings.Add(wKey,
                new OutBinding() { outputType = OutBinding.OutType.Key, outkey = 0x57 });
            wKey.Click += OutputBtn_Click;
            associatedBindings.Add(eKey,
                new OutBinding() { outputType = OutBinding.OutType.Key, outkey = 0x45 });
            eKey.Click += OutputBtn_Click;
            associatedBindings.Add(rKey,
                new OutBinding() { outputType = OutBinding.OutType.Key, outkey = 0x52 });
            rKey.Click += OutputBtn_Click;
            associatedBindings.Add(tKey,
                new OutBinding() { outputType = OutBinding.OutType.Key, outkey = 0x55 });
            tKey.Click += OutputBtn_Click;
            associatedBindings.Add(yKey,
                new OutBinding() { outputType = OutBinding.OutType.Key, outkey = 0x59 });
            yKey.Click += OutputBtn_Click;
            associatedBindings.Add(uKey,
                new OutBinding() { outputType = OutBinding.OutType.Key, outkey = 0x55 });
            uKey.Click += OutputBtn_Click;
            associatedBindings.Add(iKey,
                new OutBinding() { outputType = OutBinding.OutType.Key, outkey = 0x49 });
            iKey.Click += OutputBtn_Click;
            associatedBindings.Add(oKey,
                new OutBinding() { outputType = OutBinding.OutType.Key, outkey = 0x4F });
            oKey.Click += OutputBtn_Click;
            associatedBindings.Add(pKey,
                new OutBinding() { outputType = OutBinding.OutType.Key, outkey = 0x50 });
            pKey.Click += OutputBtn_Click;
            associatedBindings.Add(lbracketBtn,
                new OutBinding() { outputType = OutBinding.OutType.Key, outkey = 0xDB });
            lbracketBtn.Click += OutputBtn_Click;
            associatedBindings.Add(rbracketBtn,
                new OutBinding() { outputType = OutBinding.OutType.Key, outkey = 0xDD });
            rbracketBtn.Click += OutputBtn_Click;
            associatedBindings.Add(bSlashBtn,
                new OutBinding() { outputType = OutBinding.OutType.Key, outkey = 0xDC });
            bSlashBtn.Click += OutputBtn_Click;

            associatedBindings.Add(capsLBtn,
                new OutBinding() { outputType = OutBinding.OutType.Key, outkey = 0x14 });
            capsLBtn.Click += OutputBtn_Click;
            associatedBindings.Add(aKeyBtn,
                new OutBinding() { outputType = OutBinding.OutType.Key, outkey = 0x41 });
            aKeyBtn.Click += OutputBtn_Click;
            associatedBindings.Add(sBtn,
                new OutBinding() { outputType = OutBinding.OutType.Key, outkey = 0x53 });
            sBtn.Click += OutputBtn_Click;
            associatedBindings.Add(dBtn,
                new OutBinding() { outputType = OutBinding.OutType.Key, outkey = 0x44 });
            dBtn.Click += OutputBtn_Click;
            associatedBindings.Add(fBtn,
                new OutBinding() { outputType = OutBinding.OutType.Key, outkey = 0x46 });
            fBtn.Click += OutputBtn_Click;
            associatedBindings.Add(gBtn,
                new OutBinding() { outputType = OutBinding.OutType.Key, outkey = 0x47 });
            gBtn.Click += OutputBtn_Click;
            associatedBindings.Add(hBtn,
                new OutBinding() { outputType = OutBinding.OutType.Key, outkey = 0x48 });
            hBtn.Click += OutputBtn_Click;
            associatedBindings.Add(jBtn,
                new OutBinding() { outputType = OutBinding.OutType.Key, outkey = 0x48 });
            jBtn.Click += OutputBtn_Click;
            associatedBindings.Add(kBtn,
                new OutBinding() { outputType = OutBinding.OutType.Key, outkey = 0x4B });
            kBtn.Click += OutputBtn_Click;
            associatedBindings.Add(lBtn,
                new OutBinding() { outputType = OutBinding.OutType.Key, outkey = 0x4C });
            lBtn.Click += OutputBtn_Click;
            associatedBindings.Add(semicolonBtn,
                new OutBinding() { outputType = OutBinding.OutType.Key, outkey = 0xBA });
            semicolonBtn.Click += OutputBtn_Click;
            associatedBindings.Add(aposBtn,
                new OutBinding() { outputType = OutBinding.OutType.Key, outkey = 0xDE });
            aposBtn.Click += OutputBtn_Click;
            associatedBindings.Add(enterBtn,
                new OutBinding() { outputType = OutBinding.OutType.Key, outkey = 0x0D });
            enterBtn.Click += OutputBtn_Click;


        }
    }
}
