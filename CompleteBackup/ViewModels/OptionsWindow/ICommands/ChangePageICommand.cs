using CompleteBackup.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace CompleteBackup.ViewModels.ICommands
{
    internal class ChangePageICommand<T> : ICommand
    {
        public ChangePageICommand(OptionsWindowViewModel viewManager)
        {
            m_viewManager = viewManager;
        }

        private OptionsWindowViewModel m_viewManager;

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
            var viewModel = parameter as IPageViewModel;
            m_viewManager.CurrentPageViewModel = viewModel;
        }
    }
}