using RegridMapper.Core.Commands;
using System.Windows.Input;

namespace RegridMapper.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        #region Fields

        private readonly Lazy<Dictionary<Type, BaseViewModel>> _viewCache = new(() => []);

        #endregion

        #region Properties

        public object CurrentView
        {
            get => _currentView;
            set => SetProperty(ref _currentView, value);
        }
        private object _currentView;

        #endregion

        #region Commands
       
        public ICommand ChangeViewCommand { get; }

        #endregion

        #region Constructor

        public MainViewModel()
        {
            SetDefaultView();
            ChangeViewCommand = new RelayCommand(ChangeView);
        }

        #endregion

        #region Methods

        private void ChangeView(object parameter)
        {
            if (parameter is Type viewModelType && typeof(BaseViewModel).IsAssignableFrom(viewModelType))
            {
                if (!_viewCache.Value.ContainsKey(viewModelType))
                    _viewCache.Value[viewModelType] = (BaseViewModel)Activator.CreateInstance(viewModelType)!;

                CurrentView = _viewCache.Value[viewModelType]; // Preserve previous state of the ViewModel
            }
        }

        private void SetDefaultView()
        {
            try
            {
                // Set the default view model
                var defaultViewModel = new ParcelViewModel();
                ChangeView(defaultViewModel);
                CurrentView = defaultViewModel;
            }
            catch { }
        }

        #endregion

    }
}