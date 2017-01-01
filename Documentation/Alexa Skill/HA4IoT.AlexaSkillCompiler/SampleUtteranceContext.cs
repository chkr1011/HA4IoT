using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace HA4IoT.AlexaSkillCompiler
{
    public class SampleUtteranceContext
    {
        private readonly Regex _placeholderRegex = new Regex(@"\[[\w|\s|{|}|]*\]", RegexOptions.Compiled);
        private readonly List<SampleUtterance> _sampleUtterances = new List<SampleUtterance>(80000);

        public SampleUtteranceContext(string template)
        {
            _sampleUtterances.Add(new SampleUtterance { Value = template });
        }

        public void Process()
        {
            while (true)
            {
                var pendingItems = _sampleUtterances.Where(su => !su.IsFinished).ToList();
                if (!pendingItems.Any())
                {
                    return;
                }

                foreach (var pendingItem in pendingItems)
                {
                    pendingItem.IsFinished = true;

                    var placeholders = ParsePlaceholders(pendingItem.Value);
                    if (!placeholders.Any())
                    {
                        continue;
                    }

                    _sampleUtterances.Remove(pendingItem);

                    var placeholder = placeholders.First();
                    foreach (var placeHolderValue in placeholder.Values)
                    {
                        var newSampleUtterance = new SampleUtterance
                        {
                            Value = pendingItem.Value.Replace(placeholder.Key, placeHolderValue)
                        };

                        _sampleUtterances.Add(newSampleUtterance);
                    }
                }
            }
        }

        public void WriteSampleUtterancesTo(HashSet<string> target)
        {
            foreach (var sampleUtterance in _sampleUtterances)
            {
                target.Add(sampleUtterance.Value);
            }
        }

        private List<PlaceHolder> ParsePlaceholders(string sampleUtteranceTemplate)
        {
            var result = new List<PlaceHolder>();

            var matches = _placeholderRegex.Matches(sampleUtteranceTemplate);
            if (matches.Count == 0)
            {
                return result;
            }

            foreach (Match match in matches)
            {
                var placeholderValues = match.Value.Trim('[', ']').Split(new[] { '|' }, StringSplitOptions.None);

                var item = new PlaceHolder { Key = match.Value };
                foreach (var placeholderValue in placeholderValues)
                {
                    item.Values.Add(placeholderValue);
                }

                result.Add(item);
            }

            return result;
        }
    }
}
