using System.Collections.Generic;
using TpAdministration;

namespace TpAdminService.Interfaces
{
    public interface ITargetProcessService
    {
        void ConnectToTp(string tpAddress, string login, string password);
        IEnumerable<TpUser> GetAllUsers();
        IEnumerable<TpProject> GetAllProjects();

        IEnumerable<TpUser> GetTpUsersForProjects(List<string> selectedProjects, List<TpUser> allUsers, string startDate,
                                                  string endDate);

        IEnumerable<TpTimeEntry> GetTimeEntriesByDateForUsersAndProjects(List<TpUser> users, List<TpProject> projects,
                                                                         string startDate, string endDate);

        IEnumerable<TpIteration> GetIterations(string projectId);
        IEnumerable<TpUserStory> GetStoriesByIteration(string iterationId, string projectId);
    }
}