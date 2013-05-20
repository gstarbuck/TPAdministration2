using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Linq;
using TpAdminService.Interfaces;
using TpAdministration;

namespace TpAdminService
{
    public class TargetProcessService : ITargetProcessService
    {
        private string TpAddress { get; set; }
        private string Login { get; set; }
        private string Password { get; set; }

        public void ConnectToTp(string tpAddress, string login, string password)
        {
            TpAddress = tpAddress;
            Login = login;
            Password = password;
        }

        public IEnumerable<TpUser> GetTpUsersForProjects(List<string> selectedProjects, List<TpUser> allUsers,
                                                         string startDate, string endDate)
        {
            // Get all time entries for the selected period
            IEnumerable<TpTimeEntry> allTimeEntriesForDateRange = GetAllTimeEntriesByDate(allUsers, startDate, endDate);

            // Filter out all entries for projects not in the list
            List<TpTimeEntry> timeEntriesForSelectedProjects =
                allTimeEntriesForDateRange.Where(t => selectedProjects.Contains(t.ProjectId)).ToList();

            // Get the distinct list of users in these time entries
            var usersInProjects = new List<TpUser>();
            foreach (TpTimeEntry item in timeEntriesForSelectedProjects)
            {
                if (!usersInProjects.Exists(u => u.UserID == item.UserId))
                {
                    usersInProjects.Add(allUsers.Find(u => u.UserID == item.UserId));
                }
            }

            return usersInProjects.OrderBy(u => u.LastName).ToList();
        }

        public IEnumerable<TpTimeEntry> GetTimeEntriesByDateForUsersAndProjects(List<TpUser> users,
                                                                                List<TpProject> projects,
                                                                                string startDate, string endDate)
        {
            string empIdList = GetCommaSeparatedListFromEntities(users);
            string projectIdList = GetCommaSeparatedListFromEntities(projects);

            string queryString = TpAddress + "/api/v1/Times?format=xml&take=1000&where=(Date gte '" + startDate +
                                 @"') and (Date lte '" + endDate + @"') and (User.Id in (" + empIdList +
                                 ")) and (Project.Id in (" + projectIdList + "))";

            XDocument doc = GetXDocumentFromTp(queryString);

            List<TpTimeEntry> timeEntries = GetTimeTpTimeEntriesFromXDocument(users, doc);

            // Process the list, pull the Feature for each Assignable
            ProcessTimeEntries(timeEntries);

            return timeEntries;
        }

        public IEnumerable<TpUser> GetAllUsers()
        {
            string queryString = TpAddress + "/api/v1/Users?format=xml&take=1000";

            XDocument doc = GetXDocumentFromTp(queryString);

            IEnumerable<TpUser> query = from u in doc.Descendants("User")
                                        let loginElement = u.Element("Login")
                                        where loginElement != null
                                        let firstnameElement = u.Element("FirstName")
                                        where firstnameElement != null
                                        let lastnameElement = u.Element("LastName")
                                        where lastnameElement != null
                                        let emailElement = u.Element("Email")
                                        where emailElement != null
                                        select new TpUser
                                            {
                                                UserID = u.Attribute("Id").Value,
                                                Login = loginElement.Value,
                                                FirstName = firstnameElement.Value,
                                                LastName = lastnameElement.Value,
                                                Email = emailElement.Value
                                            };

            return query.OrderBy(u => u.LastName).ToList();
        }

        public IEnumerable<TpProject> GetAllProjects()
        {
            XDocument projects = GetXDocumentFromTp(TpAddress + "/api/v1/Projects?format=xml&take=1000");

            IEnumerable<TpProject> query = from p in projects.Descendants("Project")
                                           where !p.Attributes("nil").Any()
                                           select new TpProject
                                               {
                                                   ProjectId = p.Attribute("Id").Value
                                                   ,
                                                   Name = p.Attribute("Name").Value
                                                   ,
                                                   Company = GetNillableAttributeName(p.Element("Company"))
                                                   ,
                                                   Program = GetNillableAttributeName(p.Element("Program"))
                                               };
            List<TpProject> projectList = query.ToList();

            return projectList.OrderBy(p => p.Company).ToList();
        }


        public IEnumerable<TpIteration> GetIterations(string projectId)
        {
            // First, get the ACID for the project
            string acid = GetAcidForProjects(projectId);

            XDocument iterations =
                GetXDocumentFromTp(TpAddress + "/api/v1/Iterations/?acid=" + acid);

            IEnumerable<TpIteration> query = from i in iterations.Descendants("Iteration")
                                             where !i.Attributes("nil").Any()
                                             let descriptionElement = i.Element("Description")
                                             where descriptionElement != null
                                             select new TpIteration
                                                 {
                                                     IterationId = i.Attribute("Id").Value
                                                     ,
                                                     Name = i.Attribute("Name").Value
                                                     ,
                                                     Description = descriptionElement.Value
                                                     ,
                                                     StartDate = GetFormattedDate(i, "StartDate")
                                                     ,
                                                     EndDate = GetFormattedDate(i, "EndDate")
                                                 };
            var iterationList = new List<TpIteration>(query.ToList().OrderBy(i => i.Name));
            return iterationList;
        }

        public IEnumerable<TpUserStory> GetStoriesByIteration(string iterationId, string projectId)
        {
            string acid = GetAcidForProjects(projectId);

            XDocument stories = GetXDocumentFromTp(TpAddress + "/api/v1/UserStories/?acid=" + acid + "&format=xml&take=1000&where=(Iteration.Id eq " + iterationId + ")");

            IEnumerable<TpUserStory> query = from s in stories.Descendants("UserStory")
                                             let iterationElement = s.Element("Iteration")
                                             where
                                                 iterationElement != null && !iterationElement.Attributes("nil").Any() &&
                                                 iterationElement.Attribute("Id").Value == iterationId
                                             let effortElement = s.Element("Effort")
                                             where effortElement != null
                                             let effortCompletedElement = s.Element("EffortCompleted")
                                             where effortCompletedElement != null
                                             let timeSpentElement = s.Element("TimeSpent")
                                             where timeSpentElement != null
                                             let timeRemainElement = s.Element("TimeRemain")
                                             where timeRemainElement != null
                                             let initialEstimateElement = s.Element("InitialEstimate")
                                             where initialEstimateElement != null
                                             select new TpUserStory
                                                 {
                                                     UserStoryId = s.Attribute("Id").Value
                                                     ,
                                                     Name = s.Attribute("Name").Value
                                                     ,
                                                     Feature = GetFeatureStringForStory(s)
                                                     ,
                                                     Effort = effortElement.Value
                                                     ,
                                                     EffortCompleted = effortCompletedElement.Value
                                                     ,
                                                     TimeSpent = timeSpentElement.Value
                                                     ,
                                                     TimeRemain = timeRemainElement.Value
                                                     ,
                                                     InitialEstimate = initialEstimateElement.Value
                                                     ,
                                                     IterationId = iterationElement.Attribute("Id").Value
                                                     ,
                                                     IterationName = iterationElement.Attribute("Name").Value
                                                 };
            var storyList = new List<TpUserStory>(query.ToList().OrderBy(s => s.Feature).ThenBy(s => s.Name));
            return storyList;
        }

        #region Private Methods

        private IEnumerable<TpTimeEntry> GetAllTimeEntriesByDate(IEnumerable<TpUser> users, string startDate,
                                                                 string endDate)
        {
            string queryString = TpAddress + "/api/v1/Times?format=xml&take=1000&where=(Date gte '" + startDate +
                                 @"') and (Date lte '" + endDate + @"')";

            XDocument doc = GetXDocumentFromTp(queryString);

            return GetTimeTpTimeEntriesFromXDocument(users, doc);
        }

        private XDocument GetXDocumentFromTp(string queryString)
        {
            var wc = new WebClient();
            var credCache = new CredentialCache
                {
                    {
                        new Uri(TpAddress + "/api/v1/"), "Basic",
                        new NetworkCredential(Login, Password)
                    }
                };

            wc.Credentials = credCache;

            return XDocument.Load(wc.OpenRead(queryString));
        }

        private static List<TpTimeEntry> GetTimeTpTimeEntriesFromXDocument(IEnumerable<TpUser> users, XDocument doc)
        {
            IEnumerable<TpTimeEntry> query = from t in doc.Descendants("Time")
                                             let userElement = t.Element("User")
                                             where userElement != null
                                             let dateElement = t.Element("Date")
                                             where dateElement != null
                                             orderby userElement.Attribute("Id").Value, dateElement.Value
                                             let projectElement = t.Element("Project")
                                             where projectElement != null
                                             let spentElement = t.Element("Spent")
                                             where spentElement != null
                                             let customFieldsElement = t.Element("CustomFields")
                                             where customFieldsElement != null
                                             select new TpTimeEntry
                                                 {
                                                     EmployeeName =
                                                         GetEmployeeNameById(users, userElement.Attribute("Id").Value)
                                                     ,
                                                     UserId = userElement.Attribute("Id").Value
                                                     ,
                                                     Project = projectElement.Attribute("Name").Value
                                                     ,
                                                     ProjectId = projectElement.Attribute("Id").Value
                                                     ,
                                                     Role = GetEmployeeRole(userElement.Attribute("Id").Value)
                                                     ,
                                                     Date = GetFormattedDate(t)
                                                     ,
                                                     Time = spentElement.Value
                                                     ,
                                                     AssignableId = GetAssignableId(t)
                                                     ,
                                                     Comment = GetComment(t)
                                                     ,
                                                     NonBillable = (from nb in customFieldsElement.Descendants("Field")
                                                                    let customFieldsNonBillableName = nb.Element("Name")
                                                                    where
                                                                        customFieldsNonBillableName != null &&
                                                                        customFieldsNonBillableName.Value ==
                                                                        "Non Billable"
                                                                    let customFieldsNonBillableValue =
                                                                        nb.Element("Value")
                                                                    where customFieldsNonBillableValue != null
                                                                    select customFieldsNonBillableValue.Value)
                                                 .FirstOrDefault()
                                                     ,
                                                     WebShadowBucketOverride =
                                                         (from nb in customFieldsElement.Descendants("Field")
                                                          let customFieldsWsBucketName = nb.Element("Name")
                                                          where
                                                              customFieldsWsBucketName != null &&
                                                              customFieldsWsBucketName.Value ==
                                                              "WebShadow Bucket Override"
                                                          let customFieldsWsBucketValue = nb.Element("Value")
                                                          where customFieldsWsBucketValue != null
                                                          select customFieldsWsBucketValue.Value).FirstOrDefault()
                                                 };

            return query.ToList();
        }

        private void ProcessTimeEntries(List<TpTimeEntry> entries)
        {
            // Get the list of unique assignables from the entries
            List<string> assignableIds = entries.Select(e => e.AssignableId).ToList().Distinct().ToList();
            assignableIds.RemoveAll(x => x == String.Empty);
            assignableIds.RemoveAll(x => x == null);

            // Get the entity type for each Assignable
            string commaSeparatedAssignableIds = String.Join(",", assignableIds);

            string assignablesQueryString = TpAddress + "/api/v1/Assignables?format=xml&take=1000&where=(Id in (" +
                                            commaSeparatedAssignableIds + "))&include=[EntityType]";

            XDocument xdoc = GetXDocumentFromTp(assignablesQueryString);

            // Set the EntityType for each item
            var qAssignables = from a in xdoc.Descendants("Assignable")
                               let entityTypeElement = a.Element("EntityType")
                               where entityTypeElement != null
                               select new
                                   {
                                       AssignableId = a.Attribute("Id").Value,
                                       EntityType = entityTypeElement.Attribute("Name").Value
                                   };
            foreach (var item in qAssignables)
            {
                foreach (TpTimeEntry entry in entries.FindAll(e => e.AssignableId == item.AssignableId))
                {
                    entry.EntityType = item.EntityType;
                    if (entry.EntityType == "UserStory")
                    {
                        entry.UserStoryId = entry.AssignableId;
                        entry.UserStoryName = entry.Comment;
                    }
                }
            }

            // Get the UserStory for each entity that is of type Task
            List<string> taskIds =
                entries.FindAll(e => e.EntityType == "Task").Select(e => e.AssignableId).Distinct().ToList();
            taskIds.RemoveAll(x => x == String.Empty);
            taskIds.RemoveAll(x => x == null);
            if (taskIds.Any())
            {
                string commaSeparatedTaskIds = String.Join(",", taskIds);
                string tasksQueryString = TpAddress + "/api/v1/Tasks?format=xml&take=1000&where=(Id in (" +
                                          commaSeparatedTaskIds + "))&include=[UserStory]";
                XDocument tasksXDoc = GetXDocumentFromTp(tasksQueryString);
                var qTasks = from t in tasksXDoc.Descendants("Task")
                             let userStoryElement = t.Element("UserStory")
                             where userStoryElement != null
                             select new
                                 {
                                     TaskId = t.Attribute("Id").Value,
                                     UserStoryId = userStoryElement.Attribute("Id").Value,
                                     UserStoryName = userStoryElement.Attribute("Name").Value
                                 };
                foreach (var item in qTasks)
                {
                    foreach (TpTimeEntry entry in entries.FindAll(e => e.AssignableId == item.TaskId))
                    {
                        entry.UserStoryId = item.UserStoryId;
                        entry.UserStoryName = item.UserStoryName;
                    }
                }
            }

            // Now get the Feature for each UserStory
            List<string> storyIds = entries.Select(e => e.UserStoryId).Distinct().ToList();
            storyIds.RemoveAll(x => x == String.Empty);
            storyIds.RemoveAll(x => x == null);
            string commaSeparatedStoryIds = String.Join(",", storyIds);
            string storiesQueryString = TpAddress + "/api/v1/UserStories?format=xml&take=1000&where=(Id in (" +
                                        commaSeparatedStoryIds + "))&include=[Feature]";
            XDocument storiesXDoc = GetXDocumentFromTp(storiesQueryString);
            var qStories = from s in storiesXDoc.Descendants("UserStory")
                           let featureElement = s.Element("Feature")
                           where featureElement != null && featureElement.Attributes("nil").Count() != 1
                           select new
                               {
                                   UserStoryId = s.Attribute("Id").Value,
                                   FeatureId = featureElement.Attribute("Id").Value,
                                   FeatureName = featureElement.Attribute("Name").Value
                               };
            foreach (var item in qStories)
            {
                foreach (TpTimeEntry entry in entries.FindAll(e => e.UserStoryId == item.UserStoryId))
                {
                    entry.FeatureId = item.FeatureId;
                    entry.FeatureName = item.FeatureName;
                }
            }


            // TODO: grab whether the Feature is non-billable and overide
        }


        private string GetAcidForProjects(string projectId)
        {
            XDocument acidXdoc = GetXDocumentFromTp(TpAddress + "/api/v1/Context/?ids=" + projectId);
            XElement acidElement = acidXdoc.Element("Context");
            if (acidElement != null)
            {
                string acid = acidElement.Attribute("Acid").Value;
                return acid;
            }
            return string.Empty;
        }

        private static string GetFeatureStringForStory(XElement s)
        {
            XElement xElement = s.Element("Feature");
            if (xElement != null && xElement.Attributes("nil").Any())
            {
                return string.Empty;
            }
            return xElement.Attribute("Name").Value;
        }

        #endregion

        #region Utility Methods

        private static string GetNillableAttributeName(XElement element)
        {
            if (!element.Attributes("nil").Any())
            {
                return element.Attribute("Name").Value;
            }
            return "none";
        }

        private static string GetEmployeeNameById(IEnumerable<TpUser> users, string id)
        {
            TpUser user = GetUserById(users, id);
            return user.LastName + ", " + user.FirstName;
        }

        private static TpUser GetUserById(IEnumerable<TpUser> users, string id)
        {
            return users.First(u => u.UserID == id);
        }

        //TODO: fix this so it's not hardcoded, pull actual role from TP
        private static string GetEmployeeRole(string id)
        {
            if (id == "5")
            {
                return "Technical Lead";
            }
            return "Developer";
        }

        private static string GetFormattedDate(XElement t, string dateFieldName = "Date")
        {
            XElement dateElement = t.Element(dateFieldName);
            if (dateElement != null)
            {
                string dateString = dateElement.Value;

                string trimmedDate = dateString.Replace("T00:00:00", "");

                return trimmedDate;
            }
            return null;
        }

        private static string GetComment(XElement t)
        {
            XElement assignableElement = t.Element("Assignable");
            if (assignableElement != null && assignableElement.Attributes("nil").Count() == 1)
            {
                XElement descriptionElement = t.Element("Description");
                if (descriptionElement != null) return descriptionElement.Value;
            }
            else
            {
                if (assignableElement != null)
                {
                    XElement descriptionElement = t.Element("Description");
                    if (descriptionElement != null)
                        return GetComment(assignableElement.Attribute("Id").Value,
                                          assignableElement.Attribute("Name").Value, descriptionElement.Value);
                }
            }
            return null;
        }

        private static string GetComment(string assignableId, string assignableName, string description)
        {
            return assignableId + ": " + assignableName + ", " + description;
        }

        private static string GetAssignableId(XElement t)
        {
            XElement assignableElement = t.Element("Assignable");
            if (assignableElement != null && assignableElement.Attributes("nil").Count() == 1)
            {
                return String.Empty;
            }
            return assignableElement != null ? assignableElement.Attribute("Id").Value : null;
        }

        private static string GetCommaSeparatedListFromEntities<T>(List<T> entities) where T : ITpEntity
        {
            var sb = new StringBuilder();
            for (int i = 0; i < entities.Count; i++)
            {
                sb.Append(entities[i].EntityId());
                if (i < entities.Count - 1)
                {
                    sb.Append(",");
                }
            }
            return sb.ToString();
        }

        #endregion
    }
}