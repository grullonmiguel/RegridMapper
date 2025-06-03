using RegridMapper.Core.Commands;
using System.Windows.Input;

namespace RegridMapper.ViewModels
{
    public class RegridVerifyDialogViewModel : BaseDialogViewModel
    {
        public event EventHandler<bool>? ConfirmChanged;

        public ICommand YesCommand => new RelayCommand(YesSelected);

        public ICommand NoCommand => new RelayCommand(NoSelected);

        private void YesSelected()
        {
            ConfirmChanged?.Invoke(this, true);
            OkSelected();
        }

        private void NoSelected()
        {
            ConfirmChanged?.Invoke(this, false);
            OkSelected();
        }
    }
}