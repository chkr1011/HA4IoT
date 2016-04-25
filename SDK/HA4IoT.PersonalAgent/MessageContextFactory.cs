using System;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.PersonalAgent;

namespace HA4IoT.PersonalAgent
{
    public class MessageContextFactory
    {
        private static readonly char[] WordSeparator = { ' ' };

        private readonly SynonymService _synonymService;
        private MessageContext _currentContext;

        public MessageContextFactory(SynonymService synonymService)
        {
            if (synonymService == null) throw new ArgumentNullException(nameof(synonymService));

            _synonymService = synonymService;
        }

        public MessageContext Create(IInboundMessage message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            _currentContext = new MessageContext(message);

            IdentifyWords();

            IdentifyAreas();
            IdentifyComponents();
            IdentifyComponentStates();

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
                foreach (IComponentState componentState in _synonymService.GetComponentStatesBySynonym(word))
                {
                    _currentContext.IdentifiedComponentStates.Add(componentState);
                }
            }
        }
    }
}
