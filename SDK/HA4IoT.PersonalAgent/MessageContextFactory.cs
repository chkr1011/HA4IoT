using System;
using System.Collections.Generic;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Logging;
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
            IdentifyComponents();
            IdentifyComponentStates();

            Log.Verbose($"Original message = {_currentContext.OriginalMessage.Text}; Identified components = {_currentContext.IdentifiedComponentIds.Count}; Identified component states = {_currentContext.IdentifiedComponentStates.Count}");

            return _currentContext;
        }

        private void IdentifyWords()
        {
            string[] words = _currentContext.OriginalMessage.Text.Split(WordSeparator, StringSplitOptions.RemoveEmptyEntries);
            foreach (var word in words)
            {
                _currentContext.Words.Add(word);
            }
        }

        private void IdentifyComponents()
        {
            foreach (string word in _currentContext.Words)
            {
                foreach (ComponentId componentId in _synonymService.GetComponentsBySynonym(word))
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
