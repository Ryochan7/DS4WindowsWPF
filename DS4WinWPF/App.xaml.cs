using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace DS4WinWPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
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

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // Test localization
            //System.Globalization.CultureInfo ci = new System.Globalization.CultureInfo("ja");
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
                // another instance is already running if OpenExsting succeeds.
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
            //DS4Windows.Global.ProfilePath[0] = "mixed";
            //DS4Windows.Global.LoadProfile(0, false, rootHub, false, false);
            if (firstRun)
            {
                Directory.CreateDirectory(DS4Windows.Global.appdatapath);
                AttemptSave();

                Directory.CreateDirectory(DS4Windows.Global.appdatapath + @"\Profiles\");
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

            DS4Forms.MainWindow window = new DS4Forms.MainWindow(parser);
            MainWindow = window;
            window.Show();
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
            if (parser.Driverinstall)
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
            });
            controlThread.Priority = ThreadPriority.Normal;
            controlThread.IsBackground = true;
            controlThread.Start();
            while (controlThread.IsAlive)
                Thread.SpinWait(500);
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
            }
        }
    }
}
