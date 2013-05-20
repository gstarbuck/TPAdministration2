using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;

namespace TpAdministration.ViewModel
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            ////if (ViewModelBase.IsInDesignModeStatic)
            ////{
            ////    // Create design time view services and models
            ////    SimpleIoc.Default.Register<IDataService, DesignDataService>();
            ////}
            ////else
            ////{
            ////    // Create run time view services and models
            ////    SimpleIoc.Default.Register<IDataService, DataService>();
            ////}

            
            SimpleIoc.Default.Register<MmsdBudgetAnalysisVm>();
            SimpleIoc.Default.Register<MmsdStoriesByIterationVm>();
            SimpleIoc.Default.Register<SettingsViewModel>();
            SimpleIoc.Default.Register<StatusReportGenerationVm>();
            SimpleIoc.Default.Register<TimeRecordExportViewModel>();
        }

        public MmsdBudgetAnalysisVm MmsdBudgetAnalysis { get { return ServiceLocator.Current.GetInstance<MmsdBudgetAnalysisVm>(); } }

        public MmsdStoriesByIterationVm MmsdStoriesByIteration { get { return ServiceLocator.Current.GetInstance<MmsdStoriesByIterationVm>(); } }

        public SettingsViewModel Settings { get { return ServiceLocator.Current.GetInstance<SettingsViewModel>(); } }

        public StatusReportGenerationVm StatusReportGeneration { get { return ServiceLocator.Current.GetInstance<StatusReportGenerationVm>(); } }

        public TimeRecordExportViewModel TimeRecordExport { get { return ServiceLocator.Current.GetInstance<TimeRecordExportViewModel>(); } }

        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}
