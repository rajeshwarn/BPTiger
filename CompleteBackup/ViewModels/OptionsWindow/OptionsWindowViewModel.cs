using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;

using System.Windows.Threading;
using System.Threading;
using System.Windows.Input;
using CompleteBackup.ViewModels;
using CompleteBackup;
using CompleteBackup.ViewModels.ICommands;

namespace CompleteBackup.ViewModels
{
    public class OptionsWindowViewModel : ViewModelBase
    {
        public OptionsWindowViewModel()
        {
            _pageViewModelsX = new PageViewModelsTree()
            {
                Name = "Root",
                PageViewModel = new BackupGeneralOptionsViewModel(),
                PageTree = new List<PageViewModelsTree>
                {
                    new PageViewModelsTree()
                    {
                        Name = "Database",
                        PageViewModel = new BackupGeneralOptionsViewModel(),
                        PageTree = new List<PageViewModelsTree>
                        {
                            new PageViewModelsTree() { Name = "General", PageViewModel = new BackupGeneralOptionsViewModel() },
//                            new PageViewModelsTree() { Name = "Database", PageViewModel = new DatabaseEnvironmentViewModel() }
                        }
                    },
                }
            };

            CurrentPageViewModel = _pageViewModelsX.PageTree[0].PageViewModel;

            ChangePageCommand = new ChangePageICommand<object>(this);
            SavePropertiesCommand = new SavePropertiesICommand<object>(this);
            ResetPropertiesCommand = new ResetPropertiesICommand<object>(this);
            CancelPropertiesCommand = new CancelPropertiesICommand<object>(this);
        }


        public ICommand ChangePageCommand { get; private set; }
        public ICommand SavePropertiesCommand { get; private set; }
        public ICommand ResetPropertiesCommand { get; private set; } 
        public ICommand CancelPropertiesCommand { get; private set; }

        public PageViewModelsTree PageViewModelsX
        {
            get
            {
                return _pageViewModelsX;
            }
        }

        PageViewModelsTree _pageViewModelsX;

        public void SwitchToICRGenericProtocolConfigurationPage()
        {
            CurrentPageViewModel = _pageViewModelsX.PageTree[2].PageTree[2].PageViewModel;
        }

        public void SwitchToDataBasePathConfigurationPage()
        {
            CurrentPageViewModel = _pageViewModelsX.PageTree[0].PageTree[1].PageViewModel;
        }

        private IPageViewModel _currentPageViewModel;

        public IPageViewModel CurrentPageViewModel
        {
            get
            {
                return _currentPageViewModel;
            }
            set
            {
                if (_currentPageViewModel != value)
                {
                    _currentPageViewModel = value;
                    OnPropertyChanged("CurrentPageViewModel");
                }
            }
        }

    }
}
