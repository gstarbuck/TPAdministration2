using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Word;
using TpAdminService.Interfaces;
using TpAdministration;
using TpAdminService;

namespace StatusReportService
{
    public class StatusReportGenerationService : Interfaces.IStatusReportGenerationService
    {

        private ITargetProcessService _service;

        public IEnumerable<TpIteration> IterationList { get; set; }
        public IEnumerable<TpUserStory> TpUserStories { get; set; }

        public TpIteration CurrentIteration { get; set; }

        public void InitializeService(string tpAddress, string login, string password)
        {
            _service = new TargetProcessService();
            _service.ConnectToTp(tpAddress, login, password);

            InitializeLists();
            CurrentIteration = IterationList.FirstOrDefault(
                    (i) =>
                    {
                        DateTime startDate = DateTime.Parse(i.StartDate);
                        DateTime endDate = DateTime.Parse(i.EndDate);

                        return startDate < DateTime.Now && endDate > DateTime.Now;
                    });
        }


        public void GenerateDocument()
        {
            var app = new Application {Visible = true};

            object visible = true;
            object template = @"C:\Users\gstarbuck\Documents\Custom Office Templates\StatusReportTemplate.dotx";

            var doc = app.Documents.Add(Visible: ref visible, Template: ref template);

            doc.Bookmarks["bmStatusReportDate"].Range.Text = DateTime.Now.ToShortDateString();

            doc.Bookmarks["bmStatusDate"].Range.Text = DateTime.Now.ToString("D");




            object fileName = ("C:\\" + ("Temp\\" + DateTime.Now.ToString("s") + ".docx").Replace(":", "-"));
            doc.SaveAs(FileName: ref fileName);

            object saveChanges = true;

            app.Quit(SaveChanges: ref saveChanges);
        }

        private void InitializeLists()
        {
            IterationList = _service.GetIterations("12694");
            TpUserStories = _service.GetStoriesByIteration(CurrentIteration.IterationId, "12694");
        }

    }
}
