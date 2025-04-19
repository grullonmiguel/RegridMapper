using RegridMapper.Core.Commands;
using System.Windows.Input;

namespace RegridMapper.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        #region Fields

        private readonly Lazy<Dictionary<Type, BaseViewModel>> _viewCache =
            new(() => new Dictionary<Type, BaseViewModel>(), LazyThreadSafetyMode.ExecutionAndPublication);

        #endregion

        #region Properties

        public BaseViewModel CurrentViewModel
        {
            get => _currentViewModel;
            set => SetProperty(ref _currentViewModel, value);
        }
        private BaseViewModel _currentViewModel;


        #endregion

        #region Commands

        public ICommand ChangeViewCommand => new RelayCommand<Type>(viewModelType =>
        {
            Console.WriteLine($"Executing ChangeViewCommand with {viewModelType.Name}");
            CurrentViewModel = GetCachedViewModel(viewModelType);
        });


        #endregion

        #region Constructor

        public MainViewModel()
        {
            //SetDefaultView();
        }

        #endregion

        #region Methods
        private BaseViewModel GetCachedViewModel(Type viewModelType)
        {
            if (!_viewCache.Value.TryGetValue(viewModelType, out BaseViewModel cachedViewModel))
            {
                cachedViewModel = (BaseViewModel)Activator.CreateInstance(viewModelType)!;
                _viewCache.Value[viewModelType] = cachedViewModel;
            }

            Console.WriteLine($"Switching to ViewModel: {cachedViewModel.GetType().Name}");
            return cachedViewModel;
        }



        private void SetDefaultView()
        {
            try
            {
                // Set the default view model
                //ChangeView(typeof(ParcelViewModel));
            }
            catch { }
        }

        #endregion

    }
}