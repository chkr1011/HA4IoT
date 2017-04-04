using System.Collections.Generic;

namespace HA4IoT.Contracts.PersonalAgent.AmazonEcho
{
    public class SkillServiceRequestRequestIntent
    {
        public string Name { get; set; }

        public Dictionary<string, SkillServiceRequestRequestIntentSlot> Slots { get; set; } = new Dictionary<string, SkillServiceRequestRequestIntentSlot>();
    }
}
