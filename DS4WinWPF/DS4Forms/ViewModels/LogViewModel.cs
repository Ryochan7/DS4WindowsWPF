using System;
using System.Collections.ObjectModel;
using System.Windows.Data;

namespace DS4WinWPF.DS4Forms.ViewModels
{
    public class LogViewModel
    {
        private object _colLockobj = new object();
        private ObservableCollection<LogItem> logItems = new ObservableCollection<LogItem>();

        public ObservableCollection<LogItem> LogItems => logItems;

        public LogViewModel(DS4Windows.ControlService service)
        {
            //string version = DS4Windows.Global.exeversion;
            //logItems.Add(new LogItem { Datetime = DateTime.Now, Message = $"DS4Windows version {version}" });
            logItems.Add(new LogItem { Datetime = DateTime.Now, Message = "DS4Windows version 2.0" });
            BindingOperations.EnableCollectionSynchronization(logItems, _colLockobj);
            service.Debug += AddLogMessage;
            DS4Windows.AppLogger.GuiLog += AddLogMessage;
        }

        private void AddLogMessage(object sender, DS4Windows.DebugEventArgs e)
        {
            LogItem item = new LogItem { Datetime = e.Time, Message = e.Data, Warning = e.Warning };
            logItems.Add(item);
        }
    }
}
