namespace SaveOnClouds.Web.Models.CloudAPI
{
    public class ResourceStatusChangeModel
    {
        public long Id { get; set; }
        public ResourceStatus Status { get; set; } = ResourceStatus.Start;
    }

    public enum ResourceStatus
    {
        Start = 1,
        Stop = 2
    }
}