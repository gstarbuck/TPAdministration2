using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using FirstFloor.ModernUI.Windows.Controls;
using GalaSoft.MvvmLight.Command;
using TpAdminService;
using TpAdminService.Interfaces;
using TpAdministration.Properties;
using TpAdministration.View;

namespace TpAdministration.ViewModel
{
    public class TimeRecordExportViewModel
    {
        #region Command Declarations

        public RelayCommand ShowSettingsViewCommand { get; private set; }

        #endregion

        private readonly ITargetProcessService _service;


        public TimeRecordExportViewModel()
        {
            ShowSettingsViewCommand = new RelayCommand(ShowSettingsView);
            _service = new TargetProcessService();
            _service.ConnectToTp(Settings.Default.TPAddress, Settings.Default.Login, Settings.Default.Password);

            TpUsers = _service.GetAllUsers().ToList();
            AllTpUsers = TpUsers.GetRange(0, TpUsers.Count());
            Projects = _service.GetAllProjects().ToList();
        }

        public string SelectedProject { get; set; }
        public List<TpUser> TpUsers { get; set; }
        public List<TpUser> AllTpUsers { get; set; }
        public List<TpProject> Projects { get; set; }

        public static string GetDefaultStartDate()
        {
            Trace.Write("Calling GetDefaultStartDate");
            try
            {
                return DateTime.Now.StartOfWeek(DayOfWeek.Sunday).AddDays(-7).ToString("yyyy-MM-dd");
            }
            catch (Exception ex)
            {
                Trace.Write("GetDefaultStartDate exception: " + ex.Message);
                return string.Empty;
            }
        }

        public static string GetDefaultEndDate()
        {
            return DateTime.Now.StartOfWeek(DayOfWeek.Sunday).AddDays(-1).ToString("yyyy-MM-dd");
        }

        public void ShowSettingsView()
        {
            var modernDialog = new ModernDialog {Content = new SettingsView()};
            modernDialog.ShowDialog();
        }

        internal IEnumerable<TpTimeEntry> GetTimeEntriesForUsersAndProjects(List<TpUser> users, List<TpProject> projects,
                                                                            string startDate, string endDate)
        {
            return _service.GetTimeEntriesByDateForUsersAndProjects(users, projects, startDate, endDate);
        }

        internal List<TpUser> RefreshUserList(IList selectedItems, string startDate, string endDate)
        {
            var selectedProjects = new List<string>();

            foreach (object item in selectedItems)
            {
                selectedProjects.Add(((TpProject) item).ProjectId);
            }

            return
                _service.GetTpUsersForProjects(selectedProjects, AllTpUsers, startDate, endDate)
                        .OrderBy(u => u.LastName)
                        .ToList();
        }

        internal string GenerateCsv(IEnumerable<TpTimeEntry> list, string startDate, string endDate)
        {
            string folder;
            //if (System.Diagnostics.Debugger.IsAttached)
            //{
            folder = @"C:\Temp\";
            //}
            //else
            //{
            //    folder = System.Deployment.Application.ApplicationDeployment.CurrentDeployment.DataDirectory;
            //}
            string filename = folder + SelectedProject + "TeamTimeEntries " + startDate + " to " + endDate + "." +
                              DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture) + ".csv";

            if (File.Exists(filename))
            {
                File.Delete(filename);
            }
            var streamWriter = new StreamWriter(filename);
            var csvWriter = new CsvWriter(streamWriter);

            foreach (TpTimeEntry item in list)
            {
                csvWriter.WriteRecord(item);
            }

            streamWriter.Close();

            return filename;
        }
    }

    public static class DateTimeExtensions
    {
        public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = dt.DayOfWeek - startOfWeek;
            if (diff < 0)
            {
                diff += 7;
            }

            return dt.AddDays(-1*diff).Date;
        }
    }
}