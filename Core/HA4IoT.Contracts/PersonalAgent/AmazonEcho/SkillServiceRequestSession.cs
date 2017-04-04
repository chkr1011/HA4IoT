namespace HA4IoT.Contracts.PersonalAgent.AmazonEcho
{
    public class SkillServiceRequestSession
    {
        public string SessionId { get; set; }

        public SkillServiceRequestSessionApplication Application { get; set; } = new SkillServiceRequestSessionApplication();

        public SkillServiceRequestSessionUser User { get; set; } = new SkillServiceRequestSessionUser();

        public bool New { get; set; }
    }
}
