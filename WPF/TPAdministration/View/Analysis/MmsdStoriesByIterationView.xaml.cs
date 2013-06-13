using System.Windows;
using System.Windows.Controls;
using TpAdministration.ViewModel;

namespace TpAdministration.View.Analysis
{
    /// <summary>
    ///     Interaction logic for MmsdStoriesByIterationView.xaml
    /// </summary>
    public partial class MmsdStoriesByIterationView : UserControl
    {
        public MmsdStoriesByIterationView()
        {
            InitializeComponent();

            _vm = new MmsdStoriesByIterationVm();
            DataContext = _vm;
        }

        private MmsdStoriesByIterationVm _vm { get; set; }

        private void btnGetCurrentIterationStories_Click(object sender, RoutedEventArgs e)
        {
        }

        private void DgResults_OnCopyingRowClipboardContent(object sender, DataGridRowClipboardEventArgs e)
        {
            e.ClipboardRowContent.Clear();
            e.ClipboardRowContent.Add(new DataGridClipboardCellContent(e.Item, (sender as DataGrid).Columns[0], e.Item.ToString()));
        }
    }
}