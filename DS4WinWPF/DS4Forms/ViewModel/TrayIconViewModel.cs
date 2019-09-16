using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS4WinWPF.DS4Forms.ViewModel
{
    public class TrayIconViewModel
    {
        private string tooltipText = "DS4Windows";
        private string iconSource = "/DS4WinWPF;component/DS4W.ico";

        public string TooltipText { get => tooltipText;
            set
            {
                if (tooltipText == value) return;
                tooltipText = value;
                TooltipTextChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler TooltipTextChanged;
        public string IconSource { get => iconSource;
            set
            {
                if (iconSource == value) return;
                iconSource = value;
                IconSourceChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler IconSourceChanged;
    }
}
