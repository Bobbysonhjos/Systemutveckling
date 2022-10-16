 using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
 using SmartApp.MVVM.Views;

 namespace SmartApp.MVVM.ViewModels
{
    internal class MainViewModel : ObservableObject
    {
        public MainViewModel()
        {
            KitchenViewModel = new KitchenViewModel();
            BedroomViewModel = new BedroomviewModel();
            
            
            

            KitchenViewCommand = new RelayCommand(x => { CurrentView = KitchenViewModel; });
            BedroomViewCommand = new RelayCommand(x => { CurrentView = BedroomViewModel; });
            

            
            CurrentView = KitchenViewModel;


        }
           

        private object _currentView;
        public RelayCommand KitchenViewCommand { get; set; }

        public KitchenViewModel KitchenViewModel { get; set; }

        public RelayCommand BedroomViewCommand { get; set; }

        public BedroomviewModel BedroomViewModel { get; set; }

        public object CurrentView
        {
            get { return _currentView; }
            set
            {
                _currentView = value;
                OnPropertyChanged();
            }
        }
    }
}
