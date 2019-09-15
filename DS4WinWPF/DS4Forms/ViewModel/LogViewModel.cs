using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace DS4WinWPF
{
    public class LogViewModel
    {
        private object _colLockobj = new object();
        private ObservableCollection<LogItem> logItems = new ObservableCollection<LogItem>();

        public ObservableCollection<LogItem> LogItems => logItems;

        public LogViewModel()
        {
            BindingOperations.EnableCollectionSynchronization(logItems, _colLockobj);
        }
    }
}
