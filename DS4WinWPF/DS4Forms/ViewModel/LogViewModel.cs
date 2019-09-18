using System;
using System.Collections.ObjectModel;
using System.Windows.Data;

namespace DS4WinWPF.DS4Forms.ViewModel
{
    public class LogViewModel
    {
        private object _colLockobj = new object();
        private ObservableCollection<LogItem> logItems = new ObservableCollection<LogItem>();

        public ObservableCollection<LogItem> LogItems => logItems;

        public LogViewModel()
        {
            logItems.Add(new LogItem { Datetime = DateTime.Now, Message = "DS4Windows version 2.0" });
            BindingOperations.EnableCollectionSynchronization(logItems, _colLockobj);
            AppLogger.GuiLog += AddLogMessage;
        }

        private void AddLogMessage(object sender, DebugEventArgs e)
        {
            LogItem item = new LogItem { Datetime = e.Time, Message = e.Data, Warning = e.Warning };
            logItems.Add(item);
        }
    }
}
