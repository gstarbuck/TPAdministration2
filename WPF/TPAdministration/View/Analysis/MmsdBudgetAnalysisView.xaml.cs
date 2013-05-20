using TpAdministration.ViewModel;

namespace TpAdministration.View.Analysis
{
    /// <summary>
    ///     Interaction logic for MmsdBudgetAnalysisView.xaml
    /// </summary>
    public partial class MmsdBudgetAnalysisView
    {
        private readonly MmsdBudgetAnalysisVm _vm;

        public MmsdBudgetAnalysisView()
        {
            InitializeComponent();

            _vm = new MmsdBudgetAnalysisVm();
            DataContext = _vm;
        }
    }
}