using CompleteBackup.ViewModels.MainWindow;
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

namespace CompleteBackup.Views.MainWindow
{
    /// <summary>
    /// Interaction logic for MainWindowStatusBarView.xaml
    /// </summary>
    public partial class MainWindowStatusBarView : UserControl
    {
        public MainWindowStatusBarView()
        {
            InitializeComponent();

            //TODO guyprio2, this is bad!!! use events and not a direct reference pointer
            GenericStatusBarView.SetMainWindowStatusBarViewInstance(this);
        }

        public void Release(Guid guid)
        {
            MainWindowStatusBarViewModel viewModel = this.DataContext as MainWindowStatusBarViewModel;

            viewModel?.Release(guid);
        }

        public void SetIndeterminate(Guid guid, bool bIndeterminate)
        {
            MainWindowStatusBarViewModel viewModel = this.DataContext as MainWindowStatusBarViewModel;

            viewModel?.SetIndeterminate(guid, bIndeterminate);
        }

        public void SetRange(Guid guid, long range)
        {
            MainWindowStatusBarViewModel viewModel = this.DataContext as MainWindowStatusBarViewModel;

            viewModel?.SetRange(guid, range);
        }

        public void UpdateProgressBar(Guid guid, long progress)
        {
            MainWindowStatusBarViewModel viewModel = this.DataContext as MainWindowStatusBarViewModel;

            viewModel?.SetProgress(guid, progress);
        }

        public void UpdateProgressBar(Guid guid, string text, long progress)
        {
            MainWindowStatusBarViewModel viewModel = this.DataContext as MainWindowStatusBarViewModel;

            viewModel?.SetProgress(guid, progress);
            viewModel?.SetProgressText(guid, text);
        }

        public void UpdateProgressBar(Guid guid, string text)
        {
            MainWindowStatusBarViewModel viewModel = this.DataContext as MainWindowStatusBarViewModel;

            viewModel?.SetProgressText(guid, text);
        }

        public void UpdateStatusBarText(string text)
        {
            MainWindowStatusBarViewModel viewModel = this.DataContext as MainWindowStatusBarViewModel;

            viewModel?.SetStatusBarText(text);
        }

        public static void UpdateStatusTextCHANGEME(string status, bool bSameContext = false)
        {
            return;
        }

    }
}
