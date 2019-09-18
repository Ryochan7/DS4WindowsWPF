using System;
using System.Collections.Generic;
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
    /// Interaction logic for SaveWhere.xaml
    /// </summary>
    public partial class SaveWhere : Window
    {
        private bool multisaves;

        public SaveWhere(bool multisavespots)
        {
            InitializeComponent();
            multisaves = multisavespots;
            if (!multisavespots)
                multipleSavesDockP.Visibility = Visibility.Collapsed;

        }

        private void ProgFolderBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void AppdataBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
