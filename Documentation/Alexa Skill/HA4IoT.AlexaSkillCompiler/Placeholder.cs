using System.Collections.Generic;

namespace HA4IoT.AlexaSkillCompiler
{
    public class PlaceHolder
    {
        public string Key { get; set; }

        public HashSet<string> Values { get; } = new HashSet<string>();
    }
}
