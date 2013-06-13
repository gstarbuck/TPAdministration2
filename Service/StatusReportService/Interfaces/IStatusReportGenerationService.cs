using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatusReportService.Interfaces
{
    public interface IStatusReportGenerationService
    {
        void InitializeService(string tpAddress, string login, string password);
        void GenerateDocument();
    }
}
