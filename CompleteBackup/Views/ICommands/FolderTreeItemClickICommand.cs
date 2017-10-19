﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CompleteBackup.Views.ICommands
{
    public class FolderTreeItemClickICommand : ICommand
    {
        public delegate void ICommandOnExecute(object parameter);
        public delegate bool ICommandOnCanExecute(object parameter);

//        private ICommandOnExecute _execute;
//        private ICommandOnCanExecute _canExecute;

        public FolderTreeItemClickICommand()
//        public FolderTreeItemClickICommand(ICommandOnExecute onExecuteMethod, ICommandOnCanExecute onCanExecuteMethod)
        {
//            _execute = onExecuteMethod;
//            _canExecute = onCanExecuteMethod;
        }

        #region ICommand Members

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            //            return _canExecute.Invoke(parameter);
            return true;
        }

        public void Execute(object parameter)
        {
//            _execute.Invoke(parameter);
        }

        #endregion
    }


}