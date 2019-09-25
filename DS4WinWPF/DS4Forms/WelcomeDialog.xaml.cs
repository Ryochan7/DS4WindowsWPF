using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DS4WinWPF.DS4Forms
{
    /// <summary>
    /// Interaction logic for WelcomeDialog.xaml
    /// </summary>
    public partial class WelcomeDialog : Window
    {
        private const string InstallerDL =
            "https://github.com/ViGEm/ViGEmBus/releases/download/v1.16.112/ViGEmBus_Setup_1.16.115.exe";
        private const string InstFileName = "ViGEmBus_Setup_1.16.115.exe";

        Process monitorProc;

        public WelcomeDialog(bool loadConfig = false)
        {
            if (loadConfig)
            {
                DS4Windows.Global.FindConfigLocation();
                DS4Windows.Global.Load();
                //DS4Windows.Global.SetCulture(DS4Windows.Global.UseLang);
            }

            InitializeComponent();
        }

        private void FinishedBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void VigemInstallBtn_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(DS4Windows.Global.exepath + $"\\{InstFileName}"))
            {
                File.Delete(DS4Windows.Global.exepath + $"\\{InstFileName}");
            }

            /*WebClient wb = new WebClient();
            wb.DownloadFileAsync(new Uri(InstallerDL), exepath + $"\\{InstFileName}");

            wb.DownloadProgressChanged += wb_DownloadProgressChanged;
            wb.DownloadFileCompleted += wb_DownloadFileCompleted;
            */
        }

        private void Step2Btn_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("http://www.microsoft.com/accessories/en-gb/d/xbox-360-controller-for-windows");
        }

        private void BluetoothSetLink_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("control", "bthprops.cpl");
        }
    }
}
