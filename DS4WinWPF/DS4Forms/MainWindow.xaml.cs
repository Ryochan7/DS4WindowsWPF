using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DS4WinWPF.DS4Forms
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private StatusLogMsg lastLogMsg = new StatusLogMsg();
        private ProfileList profileListHolder = new ProfileList();

        public MainWindow()
        {
            InitializeComponent();

            LogViewModel logvm = new LogViewModel();
            logvm.LogItems.Add(new LogItem() { Datetime = DateTime.Now, Message = "Bacon" });
            //logListView.ItemsSource = logvm.LogItems;
            logListView.DataContext = logvm;
            lastMsgLb.DataContext = lastLogMsg;
            
            profilesListBox.ItemsSource = profileListHolder.ProfileListCol;
            
            Task.Delay(5000).ContinueWith((t) =>
            {
                logvm.LogItems.Add(new LogItem { Datetime = DateTime.Now, Message = "Next Thing" });
                profileListHolder.ProfileListCol.Add(new ProfileEntity { Name = "Media" });

                //Dispatcher.BeginInvoke((Action)(() =>
                //{
                lastLogMsg.Message = "Controller 1 Using \"dfsdfsdfs\"";
                //}));
            });


            //lastLogMsg.Message = "Controller 1 Using \"abc\"";
            
        }

        private void AboutBtn_Click(object sender, RoutedEventArgs e)
        {
            About aboutWin = new About();
            aboutWin.Owner = this;
            aboutWin.ShowDialog();
        }

        private void StartStopBtn_Click(object sender, RoutedEventArgs e)
        {
            //Dispatcher.BeginInvoke((Action)(() =>
            //{
            lastLogMsg.Message = "Controller 1 Using \"Warsow\"";
            //}));

            //lastMsgLb.IsEnabled = true;
            //Thread.Sleep(1000);
            //lastMsgLb.DataContext = lastLogMsg;
        }
    }
}
