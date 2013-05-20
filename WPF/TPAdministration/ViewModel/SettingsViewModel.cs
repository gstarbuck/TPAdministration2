using TpAdministration.Properties;

namespace TpAdministration.ViewModel
{
    public class SettingsViewModel
    {
        public SettingsViewModel()
        {
            TPSite = Settings.Default.TPAddress;
            Login = Settings.Default.Login;
            Password = Settings.Default.Password;
        }

        public string TPSite { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }

        internal void SaveSettings()
        {
            Settings.Default.TPAddress = TPSite;
            Settings.Default.Login = Login;
            Settings.Default.Password = Password;

            Settings.Default.Save();
        }
    }
}