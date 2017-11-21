using System;
using System.Windows;
using System.Windows.Input;

namespace CompleteBackup.ViewModels.ICommands
{
    internal class ResetPropertiesICommand<T> : ICommand
    {
        public ResetPropertiesICommand(OptionsWindowViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        private OptionsWindowViewModel _viewModel;

        public event EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
            }
            remove
            {
                CommandManager.RequerySuggested -= value;
            }
        }

        public bool CanExecute(object parameter)
        {
            var window = parameter as Window;

            return (window != null);
        }

        public void Execute(object parameter)
        {
            var window = parameter as Window;

            //Properties.CollectionExportSettings.Default.Reset();
            //Properties.GraphSettings.Default.Reset();

            //Properties.CaptureGenericSettings.Default.Reset();
            //Properties.JagSettings.Default.Reset();
            //Properties.ICRGenericProtocolSettings.Default.Reset();

            //Properties.MongoDBSettings.Default.Reset();
            //Properties.ProcessStartSetting.Default.Reset();
            //Properties.Settings.Default.Reset();
        }
    }
}