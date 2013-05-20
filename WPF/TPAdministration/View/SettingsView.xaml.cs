using System.Windows;
using System.Windows.Controls;
using TpAdministration.ViewModel;

namespace TpAdministration.View
{
    /// <summary>
    ///     Interaction logic for SettingsView.xaml
    /// </summary>
    public partial class SettingsView : UserControl
    {
        private readonly SettingsViewModel _viewModel;

        public SettingsView()
        {
            InitializeComponent();
            _viewModel = new SettingsViewModel();
            DataContext = _viewModel;
        }

        private void buttonSaveSettings_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.SaveSettings();
        }
    }
}