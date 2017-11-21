using CompleteBackup.ViewModels;
using System;

using System.Windows;


namespace CompleteBackup.Views
{
    /// <summary>
    /// Interaction logic for OptionsWindow.xaml
    /// </summary>
    public partial class OptionsWindow : Window
    {
        public OptionsWindow()
        {
            InitializeComponent();
        }

        public OptionsWindowViewModel ViewModel { get { return this.DataContext as OptionsWindowViewModel; } }
    }
}
