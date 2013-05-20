using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TpAdministrationTests
{
    [TestClass]
    public class StatusReportServiceTests
    {
        [TestMethod]
        public void TestWordGeneration()
        {
            var service = new StatusReportService.StatusReportGenerationService();

            service.GenerateDocument();
        }
    }
}
