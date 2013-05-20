using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Command;
using StatusReportService;
using StatusReportService.Interfaces;

namespace TpAdministration.ViewModel
{
    public class StatusReportGenerationVm
    {
        public StatusReportGenerationVm()
        {
            GenerateStatusReportCommand = new RelayCommand(ExecuteGenerateStatusReport);
        }

        public RelayCommand GenerateStatusReportCommand { get; private set; }

        private void ExecuteGenerateStatusReport()
        {
            IStatusReportGenerationService svc = new StatusReportGenerationService();

            svc.GenerateDocument();
        }
    }
}
