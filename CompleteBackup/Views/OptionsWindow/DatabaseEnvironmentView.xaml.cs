using RadAnalytics.ViewModels;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RadAnalytics.Views
{
    /// <summary>
    /// Interaction logic for DatabaseEnvironmentView.xaml
    /// </summary>
    public partial class DatabaseEnvironmentView : UserControl
    {
        public DatabaseEnvironmentView()
        {
            InitializeComponent();
        }

        private void DatabaseEnvironmentView_Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Uri.ToString());
        }
    }
}


