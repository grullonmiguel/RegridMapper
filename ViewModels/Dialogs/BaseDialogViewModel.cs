using RegridMapper.Core.Commands;
using RegridMapper.Core.Utilities;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace RegridMapper.ViewModels
{
    public class BaseDialogViewModel : Observable
    {
        #region Events

        protected Logger? _logger;
        public event EventHandler? RequestClose;

        #endregion

        #region Commands

        public ICommand? OkCommand => new RelayCommand(OkSelected);

        #endregion

        #region Properties

        public bool IsAffirmative
        {
            get => _isAffirmative;
            set => SetProperty(ref _isAffirmative, value);
        }
        private bool _isAffirmative;

        public BaseDialogViewModel? PreviousDialog
        {
            get => _previousDialog;
            set => SetProperty(ref _previousDialog, value);
        }
        private BaseDialogViewModel? _previousDialog;

        public SolidColorBrush? MainBackground
        {
            get => _mainBackground;
            set => SetProperty(ref _mainBackground, value);
        }
        private SolidColorBrush? _mainBackground = Application.Current?.TryFindResource("Brushes.DialogBackground") as SolidColorBrush;

        #endregion

        #region Constructor

        public BaseDialogViewModel()
        {
            _logger = Logger.Instance;
        }

        #endregion

        #region Methods

        public void OnRequestClose()
        {
            RequestClose?.Invoke(this, EventArgs.Empty);
        }

        protected void OkSelected()
        {
            IsAffirmative = true;
            OnRequestClose();
        }

        protected void CancelSelected()
        {
            IsAffirmative = false;
            OnRequestClose();
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Invoked when this object is being removed from the application
        /// and will be subject to garbage collection.
        /// </summary>
        public void Dispose()
        {
            this.OnDispose();
        }

        /// <summary>
        /// Child classes can override this method to perform 
        /// clean-up logic, such as removing event handlers.
        /// </summary>
        protected virtual void OnDispose()
        { }

        #endregion
    }
}