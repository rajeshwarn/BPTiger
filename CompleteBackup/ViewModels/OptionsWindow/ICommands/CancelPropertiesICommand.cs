using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace CompleteBackup.ViewModels.ICommands
{
    internal class CancelPropertiesICommand<T> : ICommand
    {
        public CancelPropertiesICommand(OptionsWindowViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        private OptionsWindowViewModel _viewModel;

        public event EventHandler CanExecuteChanged
        {
            add
            {
//                CommandManager.RequerySuggested += value;
            }
            remove
            {
//                CommandManager.RequerySuggested -= value;
            }
        }

        public bool CanExecute(object parameter)
        {            
            return true;
        }

        public void Execute(object parameter)
        {
            //Properties.JagSettings.Default.Reload();
            //Properties.MongoDBSettings.Default.Reload();
            //Properties.ProcessStartSetting.Default.Reload();
            //Properties.Settings.Default.Reload();

            var window = parameter as Window;
            if (window != null)
            {
                window.DialogResult = true;
                window.Close();
            }
        }
    }
}