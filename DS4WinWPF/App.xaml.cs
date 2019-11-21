using NLog;
using NLog.Targets.Wrappers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using WPFLocalizeExtension.Engine;

namespace DS4WinWPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        private static extern IntPtr FindWindow(string sClass, string sWindow);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, ref COPYDATASTRUCT lParam);

        [StructLayout(LayoutKind.Sequential)]
        public struct COPYDATASTRUCT
        {
            public IntPtr dwData;
            public int cbData;
            public IntPtr lpData;
        }

        private Thread controlThread;
        public Tester rootHubtest;
        public static DS4Windows.ControlService rootHub;
        public static HttpClient requestClient;
        private bool skipSave;
        private bool runShutdown;
        private Thread testThread;
        private bool exitComThread = false;
        private const string SingleAppComEventName = "{a52b5b20-d9ee-4f32-8518-307fa14aa0c6}";
        private EventWaitHandle threadComEvent = null;
        private Timer collectTimer;

        private MemoryMappedFile ipcClassNameMMF = null; // MemoryMappedFile for inter-process communication used to hold className of DS4Form window
        private MemoryMappedViewAccessor ipcClassNameMMA = null;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // Test localization
            //CultureInfo ci = new CultureInfo("ja");
            //LocalizeDictionary.Instance.SetCurrentThreadCulture = true;
            //LocalizeDictionary.Instance.Culture = ci;
            // fixes the culture in threads
            //CultureInfo.DefaultThreadCurrentCulture = ci;
            //CultureInfo.DefaultThreadCurrentUICulture = ci;
            //DS4WinWPF.Properties.Resources.Culture = ci;
            //Thread.CurrentThread.CurrentCulture = ci;
            //Thread.CurrentThread.CurrentUICulture = ci;

            ArgumentParser parser = new ArgumentParser();
            parser.Parse(e.Args);
            CheckOptions(parser);

            try
            {
                Process.GetCurrentProcess().PriorityClass =
                    ProcessPriorityClass.High;
            }
            catch { } // Ignore problems raising the priority.

            // Force Normal IO Priority
            IntPtr ioPrio = new IntPtr(2);
            DS4Windows.Util.NtSetInformationProcess(Process.GetCurrentProcess().Handle,
                DS4Windows.Util.PROCESS_INFORMATION_CLASS.ProcessIoPriority, ref ioPrio, 4);

            // Force Normal Page Priority
            IntPtr pagePrio = new IntPtr(5);
            DS4Windows.Util.NtSetInformationProcess(Process.GetCurrentProcess().Handle,
                DS4Windows.Util.PROCESS_INFORMATION_CLASS.ProcessPagePriority, ref pagePrio, 4);

            try
            {
                // another instance is already running if OpenExisting succeeds.
                threadComEvent = EventWaitHandle.OpenExisting(SingleAppComEventName,
                    System.Security.AccessControl.EventWaitHandleRights.Synchronize |
                    System.Security.AccessControl.EventWaitHandleRights.Modify);
                threadComEvent.Set();  // signal the other instance.
                threadComEvent.Close();
                Current.Shutdown();    // Quit temp instance
                return;
            }
            catch { /* don't care about errors */ }

            // Create the Event handle
            threadComEvent = new EventWaitHandle(false, EventResetMode.ManualReset, SingleAppComEventName);
            CreateTempWorkerThread();

            CreateControlService();

            runShutdown = true;

            DS4Windows.Global.FindConfigLocation();
            bool firstRun = DS4Windows.Global.firstRun;
            if (firstRun)
            {
                DS4Forms.SaveWhere savewh = new DS4Forms.SaveWhere(false);
                savewh.ShowDialog();
            }

            DS4Windows.Global.Load();
            /*var configuration = LogManager.Configuration;
            var wrapTarget = configuration.FindTargetByName<WrapperTargetBase>("logfile") as WrapperTargetBase;
            var fileTarget = wrapTarget.WrappedTarget as NLog.Targets.FileTarget;
            fileTarget.FileName = $@"{DS4Windows.Global.appdatapath}\ds4windows_log.txt";
            fileTarget.ArchiveFileName = $@"{DS4Windows.Global.appdatapath}\ds4windows_log_{{#}}.txt";
            LogManager.Configuration = configuration;
            LogManager.ReconfigExistingLoggers();
            DispatcherUnhandledException += App_DispatcherUnhandledException;
            */

            //DS4Windows.Global.ProfilePath[0] = "mixed";
            //DS4Windows.Global.LoadProfile(0, false, rootHub, false, false);
            if (firstRun)
            {
                Directory.CreateDirectory(DS4Windows.Global.appdatapath);
                AttemptSave();

                Directory.CreateDirectory(DS4Windows.Global.appdatapath + @"\Profiles\");
                Directory.CreateDirectory(DS4Windows.Global.appdatapath + @"\Macros\");
                DS4Windows.Global.SaveProfile(0, "Default");
                DS4Windows.Global.ProfilePath[0] = DS4Windows.Global.OlderProfilePath[0] = "Default";
                /*DS4Windows.Global.ProfilePath[1] = DS4Windows.Global.OlderProfilePath[1] = "Default";
                DS4Windows.Global.ProfilePath[2] = DS4Windows.Global.OlderProfilePath[2] = "Default";
                DS4Windows.Global.ProfilePath[3] = DS4Windows.Global.OlderProfilePath[3] = "Default";
                */
            }

            if (!DS4Windows.Global.LoadActions())
            {
                DS4Windows.Global.CreateStdActions();
            }

            SetUICulture(DS4Windows.Global.UseLang);
            DS4Forms.MainWindow window = new DS4Forms.MainWindow(parser);
            MainWindow = window;
            window.Show();
            HwndSource source = PresentationSource.FromVisual(window) as HwndSource;
            CreateIPCClassNameMMF(source.Handle);
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            //Console.WriteLine("App Crashed");
            //Console.WriteLine(e.Exception.StackTrace);
            //logger.Info("jghg");
            //logger.Error(e.Exception.StackTrace);
            //LogManager.Flush();
            //LogManager.Shutdown();
        }

        private void AttemptSave()
        {
            if (!DS4Windows.Global.Save()) //if can't write to file
            {
                if (MessageBox.Show("Cannot write at current location\nCopy Settings to appdata?", "DS4Windows",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    try
                    {
                        Directory.CreateDirectory(DS4Windows.Global.appDataPpath);
                        File.Copy(DS4Windows.Global.exepath + "\\Profiles.xml",
                            DS4Windows.Global.appDataPpath + "\\Profiles.xml");
                        File.Copy(DS4Windows.Global.exepath + "\\Auto Profiles.xml",
                            DS4Windows.Global.appDataPpath + "\\Auto Profiles.xml");
                        Directory.CreateDirectory(DS4Windows.Global.appDataPpath + "\\Profiles");
                        foreach (string s in Directory.GetFiles(DS4Windows.Global.exepath + "\\Profiles"))
                        {
                            File.Copy(s, DS4Windows.Global.appDataPpath + "\\Profiles\\" + Path.GetFileName(s));
                        }
                    }
                    catch { }
                    MessageBox.Show("Copy complete, please relaunch DS4Windows and remove settings from Program Directory",
                        "DS4Windows");
                }
                else
                {
                    MessageBox.Show("DS4Windows cannot edit settings here, This will now close",
                        "DS4Windows");
                }

                DS4Windows.Global.appdatapath = null;
                skipSave = true;
                Current.Shutdown();
                return;
            }
        }

        private void CheckOptions(ArgumentParser parser)
        {
            if (parser.HasErrors)
            {
                Current.Shutdown(1);
            }
            else if (parser.Driverinstall)
            {
                DS4Forms.WelcomeDialog dialog = new DS4Forms.WelcomeDialog(true);
                dialog.ShowDialog();
                Current.Shutdown();
            }
            else if (parser.ReenableDevice)
            {
                DS4Windows.DS4Devices.reEnableDevice(parser.DeviceInstanceId);
                Current.Shutdown();
            }
            else if (parser.Runtask)
            {
                StartupMethods.LaunchOldTask();
                Current.Shutdown();
            }
            else if (parser.Command)
            {
                IntPtr hWndDS4WindowsForm = IntPtr.Zero;
                hWndDS4WindowsForm = FindWindow(ReadIPCClassNameMMF(), "DS4Windows");
                if (hWndDS4WindowsForm != IntPtr.Zero)
                {
                    COPYDATASTRUCT cds;
                    cds.lpData = IntPtr.Zero;

                    try
                    {
                        cds.dwData = IntPtr.Zero;
                        cds.cbData = parser.CommandArgs.Length;
                        cds.lpData = Marshal.StringToHGlobalAnsi(parser.CommandArgs);
                        SendMessage(hWndDS4WindowsForm, DS4Forms.MainWindow.WM_COPYDATA, IntPtr.Zero, ref cds);
                    }
                    finally
                    {
                        if (cds.lpData != IntPtr.Zero)
                            Marshal.FreeHGlobal(cds.lpData);
                    }
                }

                Current.Shutdown();
            }
        }

        private void CreateControlService()
        {
            controlThread = new Thread(() => {
                rootHubtest = new Tester();
                rootHub = new DS4Windows.ControlService();
                DS4Windows.Program.rootHub = rootHub;
                requestClient = new HttpClient();
                collectTimer = new Timer(GarbageTask, null, 30000, 30000);

            });
            controlThread.Priority = ThreadPriority.Normal;
            controlThread.IsBackground = true;
            controlThread.Start();
            while (controlThread.IsAlive)
                Thread.SpinWait(500);
        }

        private void GarbageTask(object state)
        {
            GC.Collect(0, GCCollectionMode.Forced, false);
        }

        private void CreateTempWorkerThread()
        {
            testThread = new Thread(SingleAppComThread_DoWork);
            testThread.Priority = ThreadPriority.Lowest;
            testThread.IsBackground = true;
            testThread.Start();
        }

        private void SingleAppComThread_DoWork()
        {
            while (!exitComThread)
            {
                // check for a signal.
                if (threadComEvent.WaitOne())
                {
                    threadComEvent.Reset();
                    // The user tried to start another instance. We can't allow that,
                    // so bring the other instance back into view and enable that one.
                    // That form is created in another thread, so we need some thread sync magic.
                    if (!exitComThread)
                    {
                        Dispatcher.BeginInvoke((Action)(() =>
                        {
                            MainWindow.Show();
                            MainWindow.WindowState = WindowState.Normal;
                        }));
                    }
                }
            }
        }

        public void CreateIPCClassNameMMF(IntPtr hWnd)
        {
            if (ipcClassNameMMA != null) return; // Already holding a handle to MMF file. No need to re-write the data

            try
            {
                StringBuilder wndClassNameStr = new StringBuilder(128);
                if (GetClassName(hWnd, wndClassNameStr, wndClassNameStr.Capacity) != 0 && wndClassNameStr.Length > 0)
                {
                    byte[] buffer = ASCIIEncoding.ASCII.GetBytes(wndClassNameStr.ToString());

                    ipcClassNameMMF = MemoryMappedFile.CreateNew("DS4Windows_IPCClassName.dat", 128);
                    ipcClassNameMMA = ipcClassNameMMF.CreateViewAccessor(0, buffer.Length);
                    ipcClassNameMMA.WriteArray(0, buffer, 0, buffer.Length);
                    // The MMF file is alive as long this process holds the file handle open
                }
            }
            catch (Exception)
            {
                /* Eat all exceptions because errors here are not fatal for DS4Win */
            }
        }

        private string ReadIPCClassNameMMF()
        {
            MemoryMappedFile mmf = null;
            MemoryMappedViewAccessor mma = null;

            try
            {
                byte[] buffer = new byte[128];
                mmf = MemoryMappedFile.OpenExisting("DS4Windows_IPCClassName.dat");
                mma = mmf.CreateViewAccessor(0, 128);
                mma.ReadArray(0, buffer, 0, buffer.Length);
                return ASCIIEncoding.ASCII.GetString(buffer);
            }
            catch (Exception)
            {
                // Eat all exceptions
            }
            finally
            {
                if (mma != null) mma.Dispose();
                if (mmf != null) mmf.Dispose();
            }

            return null;
        }

        private void SetUICulture(string culture)
        {
            try
            {
                //CultureInfo ci = new CultureInfo("ja");
                CultureInfo ci = CultureInfo.GetCultureInfo(culture);
                LocalizeDictionary.Instance.SetCurrentThreadCulture = true;
                LocalizeDictionary.Instance.Culture = ci;
                // fixes the culture in threads
                CultureInfo.DefaultThreadCurrentCulture = ci;
                CultureInfo.DefaultThreadCurrentUICulture = ci;
                //DS4WinWPF.Properties.Resources.Culture = ci;
                Thread.CurrentThread.CurrentCulture = ci;
                Thread.CurrentThread.CurrentUICulture = ci;
            }
            catch (CultureNotFoundException) { /* Skip setting culture that we cannot set */ }
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            if (runShutdown)
            {
                Task.Run(() =>
                {
                    rootHub.Stop();
                    //rootHubtest.Stop();
                }).Wait();

                if (!skipSave)
                {
                    DS4Windows.Global.Save();
                }

                exitComThread = true;
                threadComEvent.Set();  // signal the other instance.
                while (testThread.IsAlive)
                    Thread.SpinWait(500);
                threadComEvent.Close();

                if (ipcClassNameMMA != null) ipcClassNameMMA.Dispose();
                if (ipcClassNameMMF != null) ipcClassNameMMF.Dispose();
            }
        }
    }
}
