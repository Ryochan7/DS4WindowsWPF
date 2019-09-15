using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS4WinWPF
{
    public class StatusLogMsg
    {
        private string message;
        public string Message
        {
            get => message;
            set
            {
                if (message == value) return;
                message = value;
                MessageChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler MessageChanged;
    }
}
