using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HA4IoT.AlexaSkillCompiler
{
    public static class Program
    {
        private static readonly string SampleUtterancesInputFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sample Utterances.txt");
        private static readonly string SampleUtterancesOutputFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sample Utterances.Compiled.txt");
        
        public static void Main()
        {
            try
            {
                var sampleUtterances = new HashSet<string>();

                foreach (var sampleUtteranceTemplate in LoadSampleUtteranceTemplates())
                {
                    var context = new SampleUtteranceContext(sampleUtteranceTemplate);
                    context.Process();
                    context.WriteSampleUtterancesTo(sampleUtterances);
                }

                CreateOutputFile(sampleUtterances);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }

            Console.ReadLine();
        }

        private static IEnumerable<string> LoadSampleUtteranceTemplates()
        {
            var sampleUtterances = File.ReadAllLines(SampleUtterancesInputFile);
            foreach (var sampleUtterance in sampleUtterances)
            {
                if (string.IsNullOrWhiteSpace(sampleUtterance))
                {
                    continue;
                }

                if (sampleUtterance.StartsWith("//"))
                {
                    continue;
                }

                yield return sampleUtterance;
            }
        }

        private static void CreateOutputFile(HashSet<string> sampleUtterances)
        {
            var lines = new HashSet<string>();
            foreach (var sampleUtterance in sampleUtterances)
            {
                var line = sampleUtterance;
                while (line.Contains("  "))
                {
                    line = line.Replace("  ", " ");
                }

                lines.Add(line);
            }
            
            Console.WriteLine($"Output has {lines.Count} lines.");
            File.WriteAllLines(SampleUtterancesOutputFile, lines.OrderBy(l => l));
        }
    }
}

