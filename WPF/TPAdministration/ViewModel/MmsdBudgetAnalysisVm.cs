using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Runtime.CompilerServices;
using GalaSoft.MvvmLight.Command;
using TpAdministration.Annotations;

namespace TpAdministration.ViewModel
{
    public class MmsdBudgetAnalysisVm : INotifyPropertyChanged
    {
        public MmsdBudgetAnalysisVm()
        {
            RefreshCommand = new RelayCommand(RefreshData);
        }

        public List<WsProject> ProjectsAndHours { get; set; }
        public RelayCommand RefreshCommand { get; private set; }
        public event PropertyChangedEventHandler PropertyChanged;

        public void RefreshData()
        {
            ProjectsAndHours = GetDataFromWS();
            OnPropertyChanged("ProjectsAndHours");
        }

        private static List<WsProject> GetDataFromWS()
        {
            var retval = new List<WsProject>();

            const string sql = "select pmn.Name, (sum(tamn.time)/60) from time_accounting_master_new tamn" +
                               " join project_master_new pmn on tamn.ShadowPID = pmn.ShadowPID" +
                               " where tamn.ShadowPID in(1349808343, 1349820234, 1349817397, 1349805885)" +
                               " group by pmn.Name";

            var conn =
                new SqlConnection(@"Server=yaharasql\yaharasql2005;Database=webshadow;Trusted_Connection=True;");

            var cmd = new SqlCommand(sql, conn);

            conn.Open();

            SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                retval.Add(new WsProject {HoursBurned = reader.GetInt32(1), Name = reader.GetString(0)});
            }

            conn.Close();

            return retval;
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}