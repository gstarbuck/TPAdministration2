using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Word;

namespace StatusReportService
{
    public class StatusReportGenerationService : Interfaces.IStatusReportGenerationService
    {
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
    }
}
