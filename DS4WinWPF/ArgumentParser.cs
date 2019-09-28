
namespace DS4WinWPF
{
    public class ArgumentParser
    {
        private bool mini;
        private bool stop;
        private bool driverinstall;
        private bool reenableDevice;
        private string deviceInstanceId;
        private bool runtask;
        private bool command;

        public bool Mini { get => mini; }
        public bool Stop { get => stop; }
        public bool Driverinstall { get => driverinstall; }
        public bool ReenableDevice { get => reenableDevice; }
        public bool Runtask { get => runtask; }
        public bool Command { get => command; }
        public string DeviceInstanceId { get => deviceInstanceId; }

        public void Parse(string[] args)
        {
            //foreach (string arg in args)
            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];
                switch(arg)
                {
                    case "driverinstall":
                    case "-driverinstall":
                        driverinstall = true;
                        break;

                    case "re-enabledevice":
                    case "-re-enabledevice":
                        reenableDevice = true;
                        if (i + 1 < args.Length)
                        {
                            deviceInstanceId = args[i++];
                        }

                        break;

                    case "runtask":
                    case "-runtask":
                        runtask = true;
                        break;

                    case "-stop":
                        stop = true;
                        break;

                    case "-m":
                        mini = true;
                        break;

                    case "command":
                    case "-command":
                        command = true;
                        break;

                    default: break;
                }
            }
        }
    }
}
