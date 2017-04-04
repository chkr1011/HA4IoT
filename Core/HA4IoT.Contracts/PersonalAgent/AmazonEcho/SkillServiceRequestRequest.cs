namespace HA4IoT.Contracts.PersonalAgent.AmazonEcho
{
    public class SkillServiceRequestRequest
    {
        public string Type { get; set; }

        public string RequestId { get; set; }

        public string Locale { get; set; }

        public string Timestamp { get; set; }

        public SkillServiceRequestRequestIntent Intent { get; set; } = new SkillServiceRequestRequestIntent();
    }
}
