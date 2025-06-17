using RegridMapper.Core.Commands;
using RegridMapper.Core.Services;
using System.Windows.Input;

namespace RegridMapper.ViewModels
{
    public class RegridCredentialsDialogVIewModel : BaseDialogViewModel
    {
        #region Properties

        /// <summary>
        /// Handles the Regrid user name
        /// </summary>
        public string? UserName
        {
            get => _userName;
            set => SetProperty(ref _userName, value);
        }
        private string? _userName;

        /// <summary>
        /// Handles the Regrid password
        /// </summary>
        public string? Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }
        private string? _password;

        #endregion

        #region Commands

        public ICommand SaveSettingsCommand => new RelayCommand(SaveSettings);

        #endregion

        #region Constructor

        public RegridCredentialsDialogVIewModel()
        {}

        #endregion

        #region Methods 

        public void LoadSettings()
        {
            try
            {
                UserName = SettingsService.LoadSetting<string>("User_Name");
                Password = SettingsService.LoadSetting<string>("User_Password");
            }
            catch (Exception ex)
            {
                Task.Run(async () => await _logger!.LogExceptionAsync(ex));
            }
        }

        private void SaveSettings()
        {
            try
            {
                // User Name - Allow blanks
                SettingsService.SaveSetting("User_Name", UserName);

                // Password - Allow blanks
                SettingsService.SaveSetting("User_Password", Password);

                // Close
                OkSelected();
            }
            catch (Exception ex)
            {
                Task.Run(async () => await _logger!.LogExceptionAsync(ex));
            }
        }

        #endregion
    }
}