using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace RadAnalytics.ViewModels
{
    public class PageViewModelsTree : ObservableObject
    {
        public List<PageViewModelsTree> PageTree { get; set; }

        private IPageViewModel _PageViewModel;
        public IPageViewModel PageViewModel { get { return _PageViewModel; } set { _PageViewModel = value; OnPropertyChanged("PageViewModel"); } }
        public string Name { get; set; }
    }

}
