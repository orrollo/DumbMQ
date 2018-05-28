namespace Proto01
{
    public class RouteInfo
    {
        public int Length { get; set; }

        public RouteInfo(string targetId, string nextId, int length)
        {
            TargetId = targetId;
            NextId = nextId;
            Length = length;
        }

        public string NextId { get; set; }

        public string TargetId { get; set; }
    }
}