using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using GalaSoft.MvvmLight.Command;
using TpAdminService;
using TpAdminService.Interfaces;
using TpAdministration.Annotations;
using TpAdministration.Properties;

namespace TpAdministration.ViewModel
{
    public class MmsdStoriesByIterationVm : INotifyPropertyChanged
    {
        private readonly ITargetProcessService _service;

        public RelayCommand GetStoriesCommand { get; private set; }

        public MmsdStoriesByIterationVm()
        {
            _service = new TargetProcessService();
            _service.ConnectToTp(Settings.Default.TPAddress, Settings.Default.Login, Settings.Default.Password);

            CurrentIterationList = _service.GetIterations("12694");
            //PastIterationList = _service.GetIterations("12694");

            CurrentSelectedIteration =
                CurrentIterationList.FirstOrDefault(
                    (i) =>
                        {
                            DateTime startDate = DateTime.Parse(i.StartDate);
                            DateTime endDate = DateTime.Parse(i.EndDate);

                            return startDate < DateTime.Now && endDate > DateTime.Now;
                        });

            //PastSelectedIteration = PastIterationList.FirstOrDefault(
            //    (i) =>
            //        {
            //            DateTime endDate = DateTime.Parse(i.EndDate);
            //            DateTime currentIterationStartDate = DateTime.Parse(CurrentSelectedIteration.StartDate);

            //            return endDate == currentIterationStartDate.AddDays(-1);
            //        });

            GetStoriesCommand = new RelayCommand(GetCurrentIterationStories);
        }

        public IEnumerable<TpIteration> CurrentIterationList { get; set; }
        //public IEnumerable<TpIteration> PastIterationList { get; set; }

        public TpIteration CurrentSelectedIteration { get; set; }
        //public TpIteration PastSelectedIteration { get; set; }

        public IEnumerable<TpUserStory> TpUserStories { get; set; }


        public event PropertyChangedEventHandler PropertyChanged;

        internal void GetCurrentIterationStories()
        {
            TpUserStories = _service.GetStoriesByIteration(CurrentSelectedIteration.IterationId, "12694");

            OnPropertyChanged("TpUserStories");
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}