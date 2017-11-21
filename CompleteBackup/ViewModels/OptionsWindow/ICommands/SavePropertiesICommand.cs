using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace CompleteBackup.ViewModels.ICommands
{
    internal class SavePropertiesICommand<T> : ICommand
    {
        public SavePropertiesICommand(OptionsWindowViewModel viewModel)
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

            //Properties.CollectionExportSettings.Default.Save();
            //Properties.GraphSettings.Default.Save();

            //Properties.CaptureGenericSettings.Default.Save();
            //Properties.JagSettings.Default.Save();
            //Properties.ICRGenericProtocolSettings.Default.Save();

            //Properties.MongoDBSettings.Default.Save();
            //Properties.ProcessStartSetting.Default.Save();
            //Properties.Settings.Default.Save();


            //foreach (var jagLogDetails in PumpRepository.Instance.JagLogDetailLevelsDataCollection)
            //{
            //    if ((jagLogDetails != null) && jagLogDetails.IsDirty())
            //    {
            //        IFuelGenericServiceInterface.Instance.SaveLoggingSetting(jagLogDetails);
            //    }
            //}

            window.DialogResult = true;
            window.Close();
        }
    }
}