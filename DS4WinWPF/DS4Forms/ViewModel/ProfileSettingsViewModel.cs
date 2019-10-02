using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            set => Global.MainColor[device].red = (byte)value;
        }

        public int MainColorG
        {
            get => Global.MainColor[device].green;
            set => Global.MainColor[device].green = (byte)value;
        }

        public int MainColorB
        {
            get => Global.MainColor[device].blue;
            set => Global.MainColor[device].blue = (byte)value;
        }

        public int LowColorR
        {
            get => Global.LowColor[device].red;
            set => Global.LowColor[device].red = (byte)value;
        }

        public int LowColorG
        {
            get => Global.LowColor[device].green;
            set => Global.LowColor[device].green = (byte)value;
        }

        public int LowColorB
        {
            get => Global.LowColor[device].blue;
            set => Global.LowColor[device].blue = (byte)value;
        }

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

        public int ChargingType
        {
            get => Global.ChargingType[device];
            set => Global.ChargingType[device] = value;
        }

        public double Rainbow
        {
            get => Global.Rainbow[device];
            set => Global.Rainbow[device] = value;
        }

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
        }

        public string LaunchProgram
        {
            get => Global.LaunchProgram[device];
        }

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
            }
        }

        public int IdleDisconnect
        {
            get => Global.IdleDisconnectTimeout[device];
            set
            {
                int temp = Global.IdleDisconnectTimeout[device];
                if (temp == value) return;
                Global.IdleDisconnectTimeout[device] = value;
                IdleDisconnectChanged?.Invoke(this, EventArgs.Empty);
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
                switch(Global.OutContType[device])
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
            get => (int)Global.GyroOutputMode[device];
            set
            {
                GyroOutMode temp = GyroOutMode.Controls;
                switch(value)
                {
                    case 0: break;
                    case 1:
                        temp = GyroOutMode.Mouse;
                        break;

                    case 2:
                        temp = GyroOutMode.MouseJoystick;
                        break;

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
    }
}
