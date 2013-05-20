using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Windows;
using FirstFloor.ModernUI.Windows.Controls;
using TpAdministration.Properties;
using TpAdministration.ViewModel;

namespace TpAdministration.View
{
    /// <summary>
    ///     Interaction logic for TimeRecordExportView.xaml
    /// </summary>
    public partial class TimeRecordExportView
    {
        private readonly TimeRecordExportViewModel _vm;

        public TimeRecordExportView()
        {
            InitializeComponent();
            try
            {
                if (Settings.Default.Login == string.Empty)
                {
                    var settingsView = new SettingsView();
                    var dialog = new ModernDialog {Content = settingsView};
                    dialog.ShowDialog();
                }
                _vm = new TimeRecordExportViewModel();
                DataContext = _vm;
            }
            catch (WebException we)
            {
                MessageBox.Show("Invalid TP connection information, please check your settings.  " + we.Message);
                var settingsView = new SettingsView();
                var dialog = new ModernDialog {Content = settingsView};
                dialog.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unhandled Exception: " + ex.Message);
            }
        }

        private void TimeRecordExportView_OnLoaded(object sender, RoutedEventArgs e)
        {
            TextBoxStartDate.Text = TimeRecordExportViewModel.GetDefaultStartDate();
            TextBoxEndDate.Text = TimeRecordExportViewModel.GetDefaultEndDate();
        }

        private void buttonGetTimeEntries_Click(object sender, RoutedEventArgs e)
        {
            if (LbUsers.HasItems)
            {
                if (LbUsers.SelectedItems.Count == 0)
                {
                    LbUsers.SelectAll();
                }

                List<TpUser> selectedUsers = (from object item in LbUsers.SelectedItems select item as TpUser).ToList();

                List<TpProject> selectedProjects =
                    (from object item in LbProjects.SelectedItems select item as TpProject).ToList();

                string filename =
                    _vm.GenerateCsv(
                        _vm.GetTimeEntriesForUsersAndProjects(selectedUsers, selectedProjects, TextBoxStartDate.Text,
                                                              TextBoxEndDate.Text), TextBoxStartDate.Text,
                        TextBoxEndDate.Text);

                //MessageBox.Show("Finished!");
                Process.Start(filename);
            }
            else
            {
                MessageBox.Show("Please select one or more projects and click 'Refresh' to update the list of users");
            }
        }

        #region Grid Selection Methods

        private void buttonSelectAllProjects_Click(object sender, RoutedEventArgs e)
        {
            LbProjects.SelectAll();
            //foreach (ListBoxItem item in LbProjects.Items)
            //{
            //    item.IsSelected = !item.IsSelected;
            //}
        }

        private void buttonSelectAllUsers_Click(object sender, RoutedEventArgs e)
        {
            LbUsers.SelectAll();
            //foreach (ListBoxItem item in LbUsers.Items)
            //{
            //    item.IsSelected = !item.IsSelected;
            //}
        }

        private void buttonQCWIDS_Click(object sender, RoutedEventArgs e)
        {
            LbProjects.SelectedItems.Clear();
            foreach (object item in LbProjects.Items)
            {
                if (((TpProject) item).Company == "Wisconsin Technical College Foundation")
                {
                    LbProjects.SelectedItems.Add(item);
                }
            }
            List<TpUser> userList = _vm.RefreshUserList(LbProjects.SelectedItems, TextBoxStartDate.Text,
                                                        TextBoxEndDate.Text);
            LbUsers.ItemsSource = userList;
            LbUsers.SelectAll();
            _vm.SelectedProject = "WIDS";
        }

        private void buttonQCMMSD_Click(object sender, RoutedEventArgs e)
        {
            LbProjects.SelectedItems.Clear();
            foreach (object item in LbProjects.Items)
            {
                if (((TpProject) item).Company == "Madison Metropolitan Sewerage District")
                {
                    LbProjects.SelectedItems.Add(item);
                }
            }
            List<TpUser> userList = _vm.RefreshUserList(LbProjects.SelectedItems, TextBoxStartDate.Text,
                                                        TextBoxEndDate.Text);
            LbUsers.ItemsSource = userList;
            LbUsers.SelectAll();
            _vm.SelectedProject = "MMSD";
        }

        private void ButtonMeOnly_OnClick(object sender, RoutedEventArgs e)
        {
            LbProjects.SelectedItems.Clear();
            LbProjects.SelectAll();

            foreach (object item in LbUsers.Items)
            {
                if (((TpUser) item).Login == Settings.Default.Login)
                {
                    LbUsers.SelectedItems.Add((item));
                    LbUsers.ScrollIntoView(item);
                }
            }
        }

        #endregion
    }
}