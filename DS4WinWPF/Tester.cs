using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DS4WinWPF
{
    public class Tester
    {
        private Thread tempThread;

        private bool running;

        public bool Running {
            get => running;
            set
            {
                if (running == value) return;
                running = value;
                RunningChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler RunningChanged;
        public event EventHandler StartControllers;
        public event EventHandler PreRemoveControllers;
        public event EventHandler ControllersRemoved;

        public void Start()
        {
            Running = true;
            DS4Windows.DS4Devices.isExclusiveMode = true;
            DS4Windows.DS4Devices.findControllers();
            IEnumerable<DS4Windows.DS4Device> devices = DS4Windows.DS4Devices.getDS4Controllers();
            int ind = 0;

            foreach (DS4Windows.DS4Device currentDev in devices)
            {
                currentDev.LightBarColor = new DS4Windows.DS4Color(0, 0, 255);
                currentDev.Report += ReadReport;
                // Start input data thread
                currentDev.StartUpdate();
                ind++;
            }

            StartControllers?.Invoke(this, EventArgs.Empty);
        }

        private void ReadReport(DS4Windows.DS4Device sender, EventArgs args)
        {
            DS4Windows.DS4Device current = sender;
            DS4Windows.DS4State state = current.getCurrentStateRef();
            //DS4Windows.DS4State previous = current.getPreviousStateRef();
        }

        public void Stop()
        {
            PreRemoveControllers?.Invoke(this, EventArgs.Empty);
            Running = false;
            IEnumerable<DS4Windows.DS4Device> devices = DS4Windows.DS4Devices.getDS4Controllers();
            int ind = 0;

            foreach (DS4Windows.DS4Device currentDev in devices)
            {
                currentDev.Report -= ReadReport;
                // Start input data thread
                currentDev.StopUpdate();
                ind++;
            }

            DS4Windows.DS4Devices.stopControllers();
            ControllersRemoved?.Invoke(this, EventArgs.Empty);
        }
    }
}
