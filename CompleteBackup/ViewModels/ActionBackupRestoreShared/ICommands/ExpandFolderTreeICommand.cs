using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CompleteBackup.ViewModels.FolderSelection.ICommands
{
    internal class ExpandFolderTreeICommand<T> : ICommand
    {
        public ExpandFolderTreeICommand()
        {
        }
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
            bool bExecute = false;

//            bExecute = ((EventCollectionRepository.Instance.CurrentCollection != null) && (JagEventRepository.Instance.JagEventList.Count > 0));

            return bExecute;
        }

        public void Execute(object parameter)
        {
            //var events = parameter as IList;
            //var eventList = new Collection<JagEventBase>();
            //foreach (JagEventBase e in events)
            //{
            //    eventList.Add(e);
            //}

            //JagEventRepository.Instance.DeleteJagEvents(eventList);
        }
    }
}
