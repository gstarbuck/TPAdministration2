namespace TpAdministration
{
    public class TpProject : ITpEntity
    {
        public string ProjectId { get; set; }
        public string Name { get; set; }
        public string Company { get; set; }
        public string Program { get; set; }


        public string EntityId()
        {
            return ProjectId;
        }
    }
}