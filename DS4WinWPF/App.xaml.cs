using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
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

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // Test localization
            //System.Globalization.CultureInfo ci = new System.Globalization.CultureInfo("ja");
            //DS4WinWPF.Properties.Resources.Culture = ci;
            //Thread.CurrentThread.CurrentCulture = ci;
            //Thread.CurrentThread.CurrentUICulture = ci;

            try
            {
                Process.GetCurrentProcess().PriorityClass =
                    System.Diagnostics.ProcessPriorityClass.High;
            }
            catch { } // Ignore problems raising the priority.

            CreateControlService();
            //DS4Forms.SaveWhere savewh = new DS4Forms.SaveWhere(false);
            //savewh.ShowDialog();

            DS4Windows.Global.FindConfigLocation();
            DS4Windows.Global.Load();
            DS4Windows.Global.LoadActions();
            //DS4Windows.Global.ProfilePath[0] = "mixed";
            //DS4Windows.Global.LoadProfile(0, false, rootHub, false, false);

            DS4Forms.MainWindow window = new DS4Forms.MainWindow();
            MainWindow = window;
            window.Show();
        }

        private void CreateControlService()
        {
            controlThread = new Thread(() => {
                rootHubtest = new Tester();
                rootHub = new DS4Windows.ControlService();
                DS4Windows.Program.rootHub = rootHub;
            });
            controlThread.Priority = ThreadPriority.Normal;
            controlThread.IsBackground = true;
            controlThread.Start();
            while (controlThread.IsAlive)
                Thread.SpinWait(500);
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            Task.Run(() =>
            {
                rootHub.Stop();
                //rootHubtest.Stop();
            }).Wait();

            DS4Windows.Global.Save();
        }
    }
}
