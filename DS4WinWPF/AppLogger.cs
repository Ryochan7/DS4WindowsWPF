using System;

namespace DS4WinWPF
{
    public class AppLogger
    {
        public static event EventHandler<DebugEventArgs> TrayIconLog;
        public static event EventHandler<DebugEventArgs> GuiLog;

        public static void LogToGui(string data, bool warning)
        {
            GuiLog?.Invoke(null, new DebugEventArgs(data, warning));
        }

        public static void LogToTray(string data, bool warning = false, bool ignoreSettings = false)
        {
            if (TrayIconLog != null)
            {
                if (ignoreSettings)
                    TrayIconLog(ignoreSettings, new DebugEventArgs(data, warning));
                else
                    TrayIconLog(null, new DebugEventArgs(data, warning));
            }
        }
    }

    public class DebugEventArgs : EventArgs
    {
        protected DateTime m_Time = DateTime.Now;
        protected string m_Data = string.Empty;
        protected bool warning = false;
        public DebugEventArgs(string Data, bool warn)
        {
            m_Data = Data;
            warning = warn;
        }

        public DateTime Time => m_Time;
        public string Data => m_Data;
        public bool Warning => warning;
    }
}
