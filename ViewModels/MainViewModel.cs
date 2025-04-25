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


        public BaseDialogViewModel CurrentDialogViewModel
        {
            get => _currentDialogViewModel;
            set => SetProperty(ref _currentDialogViewModel, value);
        }
        private BaseDialogViewModel _currentDialogViewModel;

        public bool IsDialogVisible
        {
            get => _isDialogVisible;
            set => SetProperty(ref _isDialogVisible, value);
        }
        private bool _isDialogVisible;

        #endregion

        #region Commands

        public ICommand ChangeViewCommand => new RelayCommand<Type>(ChangeView);

        #endregion

        #region Constructor

        public MainViewModel()
        {
            SetDefaultView();
        }

        #endregion

        #region Methods

        private void SetDefaultView()
        {
            try
            {
                ChangeView(typeof(MapViewModel));
            }
            catch { }
        }

        private void ChangeView(Type viewModelType)
        {
            CurrentViewModel = GetCachedViewModel(viewModelType);
            CurrentViewModel.OnDialogOpen -= DisplayDialog;
            CurrentViewModel.OnDialogOpen += DisplayDialog;
        }

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

        #endregion

        #region Dialog

        private void DisplayDialog(object sender, BaseDialogViewModel viewModel)
        {
            if (CurrentDialogViewModel != null)
            {
                viewModel.PreviousDialog = CurrentDialogViewModel;
            }

            CurrentDialogViewModel = viewModel;
            CurrentDialogViewModel.RequestClose -= CloseDialog;
            CurrentDialogViewModel.RequestClose += CloseDialog;

            IsDialogVisible = true;
        }

        private void CloseDialog(object? sender, EventArgs e)
        {
            if (CurrentDialogViewModel != null)
            {
                if (CurrentDialogViewModel.PreviousDialog == null)
                {
                    CurrentDialogViewModel.Dispose();
                    CurrentDialogViewModel = null;
                    IsDialogVisible = false;
                }
                else
                {
                    CurrentDialogViewModel = CurrentDialogViewModel.PreviousDialog;
                }
            }
        }

        #endregion
    }
}