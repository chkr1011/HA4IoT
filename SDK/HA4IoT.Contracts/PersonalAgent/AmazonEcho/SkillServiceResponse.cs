namespace HA4IoT.Contracts.PersonalAgent.AmazonEcho
{
    public class SkillServiceResponse
    {
        public string Version { get; set; } = "1.0";

        public SkillServiceResponseResponse Response { get; set; } = new SkillServiceResponseResponse();
    }
}
