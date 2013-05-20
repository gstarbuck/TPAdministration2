namespace TpAdministration
{
    public class TpTimeEntry
    {
        public string EmployeeName { get; set; }
        public string UserId { get; set; }
        public string Project { get; set; }
        public string ProjectId { get; set; }
        public string Role { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public string Comment { get; set; }
        public string NonBillable { get; set; }
        public string AssignableId { get; set; }
        public string EntityType { get; set; }
        public string UserStoryId { get; set; }
        public string UserStoryName { get; set; }
        public string FeatureId { get; set; }
        public string FeatureName { get; set; }
        public string WebShadowBucket { get; set; }
        public string WebShadowBucketOverride { get; set; }
    }
}