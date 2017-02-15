using Newtonsoft.Json.Linq;

namespace HA4IoT.Contracts.Components.States
{
    public class LevelState : IComponentFeatureState
    {
        public LevelState(int currentLevel)
        {
            CurrentLevel = currentLevel;
        }

        public int CurrentLevel { get; }

        public JToken Serialize()
        {
            return JObject.FromObject(this);
        }
    }
}
