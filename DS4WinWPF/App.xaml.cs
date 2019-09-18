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
        public Tester rootHub;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // Test localization
            System.Globalization.CultureInfo ci = new System.Globalization.CultureInfo("ja");
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

            DS4Forms.MainWindow window = new DS4Forms.MainWindow();
            MainWindow = window;
            window.Show();
        }

        private void CreateControlService()
        {
            controlThread = new Thread(() => { rootHub = new Tester(); });
            controlThread.Priority = ThreadPriority.Normal;
            controlThread.IsBackground = true;
            controlThread.Start();
            while (controlThread.IsAlive)
                Thread.SpinWait(500);
        }

        private async void Application_Exit(object sender, ExitEventArgs e)
        {
            await Task.Run(() =>
            {
                rootHub.Stop();
            });
        }
    }
}
