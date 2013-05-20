namespace TpAdministration
{
    public class TpUserStory
    {
        public override string ToString()
        {
            return UserStoryId + " - " + Name;
        }
        public string UserStoryId { get; set; }
        public string Name { get; set; }
        public string Effort { get; set; }
        public string EffortCompleted { get; set; }
        public string TimeSpent { get; set; }
        public string TimeRemain { get; set; }
        public string InitialEstimate { get; set; }
        public string Feature { get; set; }
        public string IterationId { get; set; }
        public string IterationName { get; set; }
    }
}