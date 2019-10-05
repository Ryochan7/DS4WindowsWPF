using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DS4Windows;

namespace DS4WinWPF.DS4Forms.ViewModel
{
    public class ProfileSettingsViewModel
    {
        private int device;
        public int Device { get => device; }

        public int MainColorR
        {
            get => Global.MainColor[device].red;
            set
            {
                Global.MainColor[device].red = (byte)value;
                MainColorRStringChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public string MainColorRString
        {
            get => $"#{ Global.MainColor[device].red.ToString("X2")}FF0000";
        }
        public event EventHandler MainColorRStringChanged;

        public int MainColorG
        {
            get => Global.MainColor[device].green;
            set
            {
                Global.MainColor[device].green = (byte)value;
                MainColorGStringChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public string MainColorGString
        {
            get => $"#{ Global.MainColor[device].green.ToString("X2")}00FF00";
        }
        public event EventHandler MainColorGStringChanged;

        public int MainColorB
        {
            get => Global.MainColor[device].blue;
            set
            {
                Global.MainColor[device].blue = (byte)value;
                MainColorBStringChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public string MainColorBString
        {
            get => $"#{ Global.MainColor[device].blue.ToString("X2")}0000FF";
        }
        public event EventHandler MainColorBStringChanged;

        public int LowColorR
        {
            get => Global.LowColor[device].red;
            set
            {
                Global.LowColor[device].red = (byte)value;
                LowColorRStringChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public string LowColorRString
        {
            get => $"#{ Global.LowColor[device].red.ToString("X2")}FF0000";
        }
        public event EventHandler LowColorRStringChanged;

        public int LowColorG
        {
            get => Global.LowColor[device].green;
            set
            {
                Global.LowColor[device].green = (byte)value;
                LowColorGStringChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public string LowColorGString
        {
            get => $"#{ Global.LowColor[device].green.ToString("X2")}FF0000";
        }
        public event EventHandler LowColorGStringChanged;

        public int LowColorB
        {
            get => Global.LowColor[device].blue;
            set
            {
                Global.LowColor[device].blue = (byte)value;
                LowColorBStringChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public string LowColorBString
        {
            get => $"#{ Global.LowColor[device].blue.ToString("X2")}FF0000";
        }
        public event EventHandler LowColorBStringChanged;

        public int FlashTypeIndex
        {
            get => Global.FlashType[device];
            set => Global.FlashType[device] = (byte)value;
        }

        public int FlashAt
        {
            get => Global.FlashAt[device];
            set => Global.FlashAt[device] = value;
        }

        public string FlashColor
        {
            get
            {
                ref DS4Color color = ref Global.FlashColor[device];
                if (color.red == 0 && color.green == 0 && color.blue == 0)
                {
                    color = ref Global.MainColor[device];
                }

                return $"#FF{color.red.ToString("X2")}{color.green.ToString("X2")}{color.blue.ToString("X2")}";
            }
        }
        public event EventHandler FlashColorChanged;

        public int ChargingType
        {
            get => Global.ChargingType[device];
            set
            {
                Global.ChargingType[device] = value;
                ChargingColorVisibleChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public bool ColorBatteryPercent
        {
            get => Global.LedAsBatteryIndicator[device];
            set
            {
                Global.LedAsBatteryIndicator[device] = value;
            }
        }

        public string ChargingColor
        {
            get
            {
                ref DS4Color color = ref Global.ChargingColor[device];
                return $"#FF{color.red.ToString("X2")}{color.green.ToString("X2")}{color.blue.ToString("X2")}";
            }
        }
        //public event EventHandler ChargingColorChanged;

        public Visibility ChargingColorVisible
        {
            get => Global.ChargingType[device] == 3 ? Visibility.Visible : Visibility.Hidden;
        }
        public event EventHandler ChargingColorVisibleChanged;

        public double Rainbow
        {
            get => Global.Rainbow[device];
            set
            {
                Global.Rainbow[device] = value;
                RainbowChanged?.Invoke(this, EventArgs.Empty);
                RainbowExistsChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler RainbowChanged;

        public bool RainbowExists
        {
            get => Global.Rainbow[device] != 0.0;
        }

        public event EventHandler RainbowExistsChanged;

        public double MaxSatRainbow
        {
            get => Global.MaxSatRainbow[device] * 100.0;
            set => Global.MaxSatRainbow[device] = value / 100.0;
        }

        public int RumbleBoost
        {
            get => Global.RumbleBoost[device];
            set => Global.RumbleBoost[device] = (byte)value;
        }

        public bool MouseAcceleration
        {
            get => Global.MouseAccel[device];
            set => Global.MouseAccel[device] = value;
        }

        public bool EnableTouchpadToggle
        {
            get => Global.EnableTouchToggle[device];
            set => Global.EnableTouchToggle[device] = value;
        }

        public bool LaunchProgramExists
        {
            get => !string.IsNullOrEmpty(Global.LaunchProgram[device]);
            set
            {
                if (!value) ResetLauchProgram();
            }
        }
        public event EventHandler LaunchProgramExistsChanged;

        public string LaunchProgram
        {
            get => Global.LaunchProgram[device];
        }
        public event EventHandler LaunchProgramChanged;

        public string LaunchProgramName
        {
            get
            {
                string temp = Global.LaunchProgram[device];
                if (!string.IsNullOrEmpty(temp))
                {
                    temp = Path.GetFileNameWithoutExtension(temp);
                }
                else
                {
                    temp = "Browse";
                }

                return temp;
            }
        }
        public event EventHandler LaunchProgramNameChanged;

        public ImageSource LaunchProgramIcon
        {
            get
            {
                ImageSource exeicon = null;
                string path = Global.LaunchProgram[device];
                if (File.Exists(path) && Path.GetExtension(path) == ".exe")
                {
                    using (Icon ico = Icon.ExtractAssociatedIcon(path))
                    {
                        exeicon = Imaging.CreateBitmapSourceFromHIcon(ico.Handle, Int32Rect.Empty,
                            BitmapSizeOptions.FromEmptyOptions());
                        exeicon.Freeze();
                    }
                }

                return exeicon;
            }
        }
        public event EventHandler LaunchProgramIconChanged;

        public bool UseDInputOnly
        {
            get => Global.useDInputOnly[device];
            set => Global.useDInputOnly[device] = value;
        }

        public bool FlushHid
        {
            get => Global.FlushHIDQueue[device];
            set => Global.FlushHIDQueue[device] = value;
        }

        public bool IdleDisconnectExists
        {
            get => Global.IdleDisconnectTimeout[device] != 0;
            set
            {
                Global.IdleDisconnectTimeout[device] = 5;
                IdleDisconnectChanged?.Invoke(this, EventArgs.Empty);
                IdleDisconnectExistsChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler IdleDisconnectExistsChanged;

        public int IdleDisconnect
        {
            get => Global.IdleDisconnectTimeout[device];
            set
            {
                int temp = Global.IdleDisconnectTimeout[device];
                if (temp == value) return;
                Global.IdleDisconnectTimeout[device] = value;
                IdleDisconnectChanged?.Invoke(this, EventArgs.Empty);
                IdleDisconnectExistsChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler IdleDisconnectChanged;

        public int BTPollRateIndex
        {
            get => Global.BTPollRate[device];
        }

        public int ControllerTypeIndex
        {
            get
            {
                int type = 0;
                switch (Global.OutContType[device])
                {
                    case OutContType.X360:
                        type = 0;
                        break;

                    case OutContType.DS4:
                        type = 1;
                        break;

                    default: break;
                }

                return type;
            }
        }

        public int GyroOutModeIndex
        {
            get
            {
                int index = 0;
                switch (Global.GyroOutputMode[device])
                {
                    case GyroOutMode.Controls:
                        index = 0; break;
                    case GyroOutMode.Mouse:
                        index = 1; break;
                    case GyroOutMode.MouseJoystick:
                        index = 2; break;
                    default: break;
                }

                return index;
            }
            set
            {
                GyroOutMode temp = GyroOutMode.Controls;
                switch(value)
                {
                    case 0: break;
                    case 1:
                        temp = GyroOutMode.Mouse; break;
                    case 2:
                        temp = GyroOutMode.MouseJoystick; break;
                    default: break;
                }

                Global.GyroOutputMode[device] = temp;
            }
        }

        public int SASteeringWheelEmulationAxisIndex
        {
            get => (int)Global.SASteeringWheelEmulationAxis[device];
            set => Global.SASteeringWheelEmulationAxis[device] = (SASteeringWheelEmulationAxisType)value;
        }

        private int[] saSteeringRangeValues =
            new int[9] { 90, 180, 270, 360, 450, 720, 900, 1080, 1440 };
        public int SASteeringWheelEmulationRangeIndex
        {
            get
            {
                int index = 360;
                switch(Global.SASteeringWheelEmulationRange[device])
                {
                    case 90:
                        index = 0; break;
                    case 180:
                        index = 1; break;
                    case 270:
                        index = 2; break;
                    case 360:
                        index = 3; break;
                    case 450:
                        index = 4; break;
                    case 720:
                        index = 5; break;
                    case 900:
                        index = 6; break;
                    case 1080:
                        index = 7; break;
                    case 1440:
                        index = 8; break;
                    default: break;
                }

                return index;
            }
            set
            {
                int temp = saSteeringRangeValues[value];
                Global.SASteeringWheelEmulationRange[device] = temp;
            }
        }

        public int SASteeringWheelEmulationRange
        {
            get => Global.SASteeringWheelEmulationRange[device];
            set => Global.SASteeringWheelEmulationRange[device] = value;
        }

        public double LSDeadZone
        {
            get => Global.LSModInfo[device].deadZone / 127d;
            set => Global.LSModInfo[device].deadZone = (int)(value * 127d);
        }

        public double RSDeadZone
        {
            get => Global.RSModInfo[device].deadZone / 127d;
            set => Global.RSModInfo[device].deadZone = (int)(value * 127d);
        }

        public double LSMaxZone
        {
            get => Global.LSModInfo[device].maxZone / 100.0;
            set => Global.LSModInfo[device].maxZone = (int)(value * 100.0);
        }

        public double RSMaxZone
        {
            get => Global.RSModInfo[device].maxZone / 100.0;
            set => Global.RSModInfo[device].maxZone = (int)(value * 100.0);
        }

        public double LSAntiDeadZone
        {
            get => Global.LSModInfo[device].antiDeadZone / 100.0;
            set => Global.LSModInfo[device].antiDeadZone = (int)(value * 100.0);
        }

        public double RSAntiDeadZone
        {
            get => Global.RSModInfo[device].antiDeadZone / 100.0;
            set => Global.RSModInfo[device].antiDeadZone = (int)(value * 100.0);
        }

        public double LSSens
        {
            get => Global.LSSens[device];
            set => Global.LSSens[device] = value;
        }

        public double RSSens
        {
            get => Global.RSSens[device];
            set => Global.RSSens[device] = value;
        }

        public bool LSSquareStick
        {
            get => Global.SquStickInfo[device].lsMode;
            set => Global.SquStickInfo[device].lsMode = value;
        }

        public bool RSSquareStick
        {
            get => Global.SquStickInfo[device].rsMode;
            set => Global.SquStickInfo[device].rsMode = value;
        }

        public double LSSquareRoundness
        {
            get => Global.SquStickInfo[device].lsRoundness;
            set => Global.SquStickInfo[device].lsRoundness = value;
        }

        public double RSSquareRoundness
        {
            get => Global.SquStickInfo[device].rsRoundness;
            set => Global.SquStickInfo[device].rsRoundness = value;
        }

        public int LSOutputCurveIndex
        {
            get => Global.getLsOutCurveMode(device);
            set
            {
                Global.setLsOutCurveMode(device, value);
                LSCustomCurveSelectedChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public int RSOutputCurveIndex
        {
            get => Global.getRsOutCurveMode(device);
            set
            {
                Global.setRsOutCurveMode(device, value);
                RSCustomCurveSelectedChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public int LSCurve
        {
            get => Global.LSCurve[device];
            set => Global.LSCurve[device] = value;
        }

        public int RSCurve
        {
            get => Global.RSCurve[device];
            set => Global.RSCurve[device] = value;
        }

        public double LSRotation
        {
            get => Global.LSRotation[device];
            set => Global.LSRotation[device] = value;
        }

        public double RSRotation
        {
            get => Global.RSRotation[device];
            set => Global.RSRotation[device] = value;
        }

        public bool LSCustomCurveSelected
        {
            get => Global.getLsOutCurveMode(device) == 6;
        }
        public event EventHandler LSCustomCurveSelectedChanged;

        public bool RSCustomCurveSelected
        {
            get => Global.getRsOutCurveMode(device) == 6;
        }
        public event EventHandler RSCustomCurveSelectedChanged;

        public string LSCustomCurve
        {
            get => Global.lsOutBezierCurveObj[device].CustomDefinition;
            set => Global.lsOutBezierCurveObj[device].InitBezierCurve(value, BezierCurve.AxisType.LSRS, true);
        }

        public string RSCustomCurve
        {
            get => Global.rsOutBezierCurveObj[device].CustomDefinition;
            set => Global.rsOutBezierCurveObj[device].InitBezierCurve(value, BezierCurve.AxisType.LSRS, true);
        }

        public double L2DeadZone
        {
            get => Global.L2ModInfo[device].deadZone / 255.0;
            set => Global.L2ModInfo[device].deadZone = (byte)(value * 255.0);
        }

        public double R2DeadZone
        {
            get => Global.R2ModInfo[device].deadZone / 255.0;
            set => Global.R2ModInfo[device].deadZone = (byte)(value * 255.0);
        }

        public double L2MaxZone
        {
            get => Global.L2ModInfo[device].maxZone / 100.0;
            set => Global.L2ModInfo[device].maxZone = (int)(value * 100.0);
        }

        public double R2MaxZone
        {
            get => Global.R2ModInfo[device].maxZone / 100.0;
            set => Global.R2ModInfo[device].maxZone = (int)(value * 100.0);
        }

        public double L2AntiDeadZone
        {
            get => Global.L2ModInfo[device].antiDeadZone / 100.0;
            set => Global.L2ModInfo[device].antiDeadZone = (int)(value * 100.0);
        }

        public double R2AntiDeadZone
        {
            get => Global.R2ModInfo[device].antiDeadZone / 100.0;
            set => Global.R2ModInfo[device].antiDeadZone = (int)(value * 100.0);
        }

        public double L2Sens
        {
            get => Global.L2Sens[device];
            set => Global.L2Sens[device] = value;
        }

        public double R2Sens
        {
            get => Global.R2Sens[device];
            set => Global.R2Sens[device] = value;
        }

        public int L2OutputCurveIndex
        {
            get => Global.getL2OutCurveMode(device);
            set
            {
                Global.setL2OutCurveMode(device, value);
                L2CustomCurveSelectedChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public int R2OutputCurveIndex
        {
            get => Global.getR2OutCurveMode(device);
            set
            {
                Global.setR2OutCurveMode(device, value);
                R2CustomCurveSelectedChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public bool L2CustomCurveSelected
        {
            get => Global.getL2OutCurveMode(device) == 6;
        }
        public event EventHandler L2CustomCurveSelectedChanged;

        public bool R2CustomCurveSelected
        {
            get => Global.getR2OutCurveMode(device) == 6;
        }
        public event EventHandler R2CustomCurveSelectedChanged;

        public string L2CustomCurve
        {
            get => Global.l2OutBezierCurveObj[device].CustomDefinition;
            set => Global.l2OutBezierCurveObj[device].InitBezierCurve(value, BezierCurve.AxisType.L2R2, true);
        }

        public string R2CustomCurve
        {
            get => Global.r2OutBezierCurveObj[device].CustomDefinition;
            set => Global.r2OutBezierCurveObj[device].InitBezierCurve(value, BezierCurve.AxisType.L2R2, true);
        }

        public double SXDeadZone
        {
            get => Global.SXDeadzone[device];
            set => Global.SXDeadzone[device] = value;
        }

        public double SZDeadZone
        {
            get => Global.SZDeadzone[device];
            set => Global.SZDeadzone[device] = value;
        }

        public double SXMaxZone
        {
            get => Global.SXMaxzone[device];
            set => Global.SXMaxzone[device] = value;
        }

        public double SZMaxZone
        {
            get => Global.SZMaxzone[device];
            set => Global.SZMaxzone[device] = value;
        }

        public double SXAntiDeadZone
        {
            get => Global.SXAntiDeadzone[device];
            set => Global.SXAntiDeadzone[device] = value;
        }

        public double SZAntiDeadZone
        {
            get => Global.SZAntiDeadzone[device];
            set => Global.SZAntiDeadzone[device] = value;
        }

        public double SXSens
        {
            get => Global.SXSens[device];
            set => Global.SXSens[device] = value;
        }

        public double SZSens
        {
            get => Global.SZSens[device];
            set => Global.SZSens[device] = value;
        }

        public int SXOutputCurveIndex
        {
            get => Global.getSXOutCurveMode(device);
            set
            {
                Global.setSXOutCurveMode(device, value);
                SXCustomCurveSelectedChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public int SZOutputCurveIndex
        {
            get => Global.getSZOutCurveMode(device);
            set
            {
                Global.setSZOutCurveMode(device, value);
                SZCustomCurveSelectedChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public bool SXCustomCurveSelected
        {
            get => Global.getSXOutCurveMode(device) == 6;
        }
        public event EventHandler SXCustomCurveSelectedChanged;

        public bool SZCustomCurveSelected
        {
            get => Global.getSZOutCurveMode(device) == 6;
        }
        public event EventHandler SZCustomCurveSelectedChanged;

        public string SXCustomCurve
        {
            get => Global.sxOutBezierCurveObj[device].CustomDefinition;
            set => Global.sxOutBezierCurveObj[device].InitBezierCurve(value, BezierCurve.AxisType.SA, true);
        }

        public string SZCustomCurve
        {
            get => Global.szOutBezierCurveObj[device].CustomDefinition;
            set => Global.szOutBezierCurveObj[device].InitBezierCurve(value, BezierCurve.AxisType.SA, true);
        }

        public bool UseTouchMouse
        {
            get => !Global.UseTPforControls[device];
            set
            {
                bool temp = !Global.UseTPforControls[device];
                if (temp == value) return;
                Global.UseTPforControls[device] = !value;
                UseTouchMouseChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler UseTouchMouseChanged;

        public bool UseTouchControls
        {
            get => Global.UseTPforControls[device];
            set
            {
                bool temp = Global.UseTPforControls[device];
                if (temp == value) return;
                Global.UseTPforControls[device] = value;
                UseTouchControlsChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler UseTouchControlsChanged;

        public bool TouchSenExists
        {
            get => Global.TouchSensitivity[device] != 0;
            set
            {
                Global.TouchSensitivity[device] = value ? (byte)100 : (byte)0;
                TouchSenExistsChanged?.Invoke(this, EventArgs.Empty);
                TouchSensChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler TouchSenExistsChanged;

        public int TouchSens
        {
            get => Global.TouchSensitivity[device];
            set
            {
                int temp = Global.TouchSensitivity[device];
                if (temp == value) return;
                Global.TouchSensitivity[device] = (byte)value;
                if (value == 0) TouchSenExistsChanged?.Invoke(this, EventArgs.Empty);
                TouchSensChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler TouchSensChanged;

        public bool TouchScrollExists
        {
            get => Global.ScrollSensitivity[device] != 0;
            set
            {
                Global.ScrollSensitivity[device] = value ? (byte)100 : (byte)0;
                TouchScrollExistsChanged?.Invoke(this, EventArgs.Empty);
                TouchScrollChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler TouchScrollExistsChanged;

        public int TouchScroll
        {
            get => Global.ScrollSensitivity[device];
            set
            {
                int temp = Global.ScrollSensitivity[device];
                if (temp == value) return;
                Global.ScrollSensitivity[device] = value;
                if (value == 0) TouchScrollExistsChanged?.Invoke(this, EventArgs.Empty);
                TouchScrollChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler TouchScrollChanged;

        public bool TouchTapExists
        {
            get => Global.TapSensitivity[device] != 0;
            set
            {
                Global.TapSensitivity[device] = value ? (byte)100 : (byte)0;
                TouchTapExistsChanged?.Invoke(this, EventArgs.Empty);
                TouchTapChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler TouchTapExistsChanged;

        public int TouchTap
        {
            get => Global.TapSensitivity[device];
            set
            {
                int temp = Global.TapSensitivity[device];
                if (temp == value) return;
                Global.TapSensitivity[device] = (byte)value;
                if (value == 0) TouchTapExistsChanged?.Invoke(this, EventArgs.Empty);
                TouchTapChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler TouchTapChanged;

        public bool TouchDoubleTap
        {
            get => Global.DoubleTap[device];
            set
            {
                Global.DoubleTap[device] = value;
            }
        }
        
        public bool TouchJitter
        {
            get => Global.TouchpadJitterCompensation[device];
            set => Global.TouchpadJitterCompensation[device] = value;
        }

        private int[] touchpadInvertToValue = new int[4] { 0, 2, 1, 3 };
        public int TouchInvertIndex
        {
            get
            {
                int invert = Global.TouchpadInvert[device];
                int index = Array.IndexOf(touchpadInvertToValue, invert);
                return index;
            }
            set
            {
                int invert = touchpadInvertToValue[value];
                Global.TouchpadInvert[device] = invert;
            }
        }

        public bool LowerRightTouchRMB
        {
            get => Global.LowerRCOn[device];
            set
            {
                Global.LowerRCOn[device] = value;
            }
        }

        public bool StartTouchpadOff
        {
            get => Global.StartTouchpadOff[device];
            set
            {
                Global.StartTouchpadOff[device] = value;
            }
        }

        public bool TouchTrackball
        {
            get => Global.TrackballMode[device];
            set => Global.TrackballMode[device] = value;
        }

        public double TouchTrackballFriction
        {
            get => Global.TrackballFriction[device];
            set => Global.TrackballFriction[device] = value;
        }



        public bool GyroMouseTurns
        {
            get => Global.GyroMouseStickTriggerTurns[device];
            set => Global.GyroMouseStickTriggerTurns[device] = value;
        }

        public ProfileSettingsViewModel(int device)
        {
            this.device = device;
        }

        public void UpdateFlashColor(System.Windows.Media.Color color)
        {
            Global.FlashColor[device] = new DS4Color() { red = color.R, green = color.G, blue = color.B };
            FlashColorChanged?.Invoke(this, EventArgs.Empty);
        }

        public void UpdateLaunchProgram(string path)
        {
            Global.LaunchProgram[device] = path;
            LaunchProgramExistsChanged?.Invoke(this, EventArgs.Empty);
            LaunchProgramChanged?.Invoke(this, EventArgs.Empty);
            LaunchProgramNameChanged?.Invoke(this, EventArgs.Empty);
            LaunchProgramIconChanged?.Invoke(this, EventArgs.Empty);
        }

        public void ResetLauchProgram()
        {
            Global.LaunchProgram[device] = string.Empty;
            LaunchProgramExistsChanged?.Invoke(this, EventArgs.Empty);
            LaunchProgramChanged?.Invoke(this, EventArgs.Empty);
            LaunchProgramNameChanged?.Invoke(this, EventArgs.Empty);
            LaunchProgramIconChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
