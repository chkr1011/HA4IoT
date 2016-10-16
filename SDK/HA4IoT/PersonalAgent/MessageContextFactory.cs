using System;
using System.Linq;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.PersonalAgent;

namespace HA4IoT.PersonalAgent
{
    public class MessageContextFactory
    {
        private static readonly char[] WordSeparator = { ' ' };

        private readonly SynonymService _synonymService;
        private readonly IAreaService _areaService;

        private MessageContext _currentContext;

        public MessageContextFactory(SynonymService synonymService, IAreaService areaService)
        {
            if (synonymService == null) throw new ArgumentNullException(nameof(synonymService));
            if (areaService == null) throw new ArgumentNullException(nameof(areaService));

            _synonymService = synonymService;
            _areaService = areaService;
        }

        public MessageContext Create(IInboundMessage message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            _currentContext = new MessageContext(message);

            IdentifyWords();

            IdentifyAreas();
            IdentifyComponents();
            IdentifyComponentStates();
            FilterComponentIds();

            return _currentContext;
        }

        private void IdentifyWords()
        {
            string phrase = _currentContext.OriginalMessage.Text;
            phrase = phrase.Replace("?", string.Empty);
            phrase = phrase.Replace("!", string.Empty);
            phrase = phrase.Replace(".", string.Empty);
            
            string[] words = phrase.Split(WordSeparator, StringSplitOptions.RemoveEmptyEntries);
            foreach (var word in words)
            {
                _currentContext.Words.Add(word);
            }
        }

        private void IdentifyAreas()
        {
            foreach (string word in _currentContext.Words)
            {
                foreach (AreaId areaId in _synonymService.GetAreaIdsBySynonym(word))
                {
                    _currentContext.IdentifiedAreaIds.Add(areaId);
                }
            }
        }

        private void IdentifyComponents()
        {
            foreach (string word in _currentContext.Words)
            {
                foreach (ComponentId componentId in _synonymService.GetComponentIdsBySynonym(word))
                {
                    _currentContext.IdentifiedComponentIds.Add(componentId);
                }
            }
        }

        private void IdentifyComponentStates()
        {
            foreach (string word in _currentContext.Words)
            {
                foreach (ComponentState componentState in _synonymService.GetComponentStatesBySynonym(word))
                {
                    _currentContext.IdentifiedComponentStates.Add(componentState);
                }
            }
        }

        private void FilterComponentIds()
        {
            if (!_currentContext.IdentifiedComponentIds.Any())
            {
                return;
            }

            if (_currentContext.IdentifiedComponentIds.Count == 1)
            {
                _currentContext.FilteredComponentIds.Add(_currentContext.IdentifiedComponentIds.First());
                return;
            }

            foreach (var componentId in _currentContext.IdentifiedComponentIds)
            {
                foreach (var areaId in _currentContext.IdentifiedAreaIds)
                {
                    var area = _areaService.GetArea(areaId);

                    if (area.ContainsComponent(componentId))
                    {
                        _currentContext.FilteredComponentIds.Add(componentId);
                        break;
                    }
                }
            }
        }
    }
}
