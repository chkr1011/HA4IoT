namespace HA4IoT.AlexaSkillCompiler
{
    public class SampleUtterance
    {
        public bool IsFinished { get; set; }

        public string Value { get; set; }

        public override string ToString()
        {
            return $"{Value} (Finished: {IsFinished})";
        }
    }
}
