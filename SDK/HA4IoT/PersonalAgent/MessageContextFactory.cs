using System;
using System.Collections.Generic;
using System.Linq;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.PersonalAgent.AmazonEcho;
using HA4IoT.Contracts.Services.Settings;

namespace HA4IoT.PersonalAgent
{
    public class MessageContextFactory
    {
        // TODO: Move to settings and add UI.
        private readonly Dictionary<string, ComponentState> _stateSynonyms = new Dictionary<string, ComponentState>
        {
            { "ein", BinaryStateId.On },
            { "an", BinaryStateId.On },
            { "ab", BinaryStateId.Off },
            { "aus", BinaryStateId.Off },
            { "öffne", RollerShutterStateId.MovingUp },
            { "auf", RollerShutterStateId.MovingUp },
            { "hoch", RollerShutterStateId.MovingUp },
            { "rauf", RollerShutterStateId.MovingUp },
            { "runter", RollerShutterStateId.MovingDown },
            { "herunter", RollerShutterStateId.MovingDown },
            { "zu", RollerShutterStateId.MovingDown },
            { "schließe", RollerShutterStateId.MovingDown }
        };

        private readonly IAreaService _areaService;
        private readonly IComponentService _componentService;
        private readonly ISettingsService _settingsService;

        private MessageContext _currentContext;

        public MessageContextFactory(IAreaService areaService, IComponentService componentService, ISettingsService settingsService)
        {
            if (areaService == null) throw new ArgumentNullException(nameof(areaService));
            if (componentService == null) throw new ArgumentNullException(nameof(componentService));
            if (settingsService == null) throw new ArgumentNullException(nameof(settingsService));

            _areaService = areaService;
            _componentService = componentService;
            _settingsService = settingsService;
        }

        public MessageContext Create(string text)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));

            _currentContext = new MessageContext { Text = text };

            IdentifyAreas(text);
            IdentifyComponents(text);
            IdentifyComponentStates(text);
            FilterComponentIds();

            return _currentContext;
        }

        public MessageContext Create(SkillServiceRequest skillServiceRequest)
        {
            if (skillServiceRequest == null) throw new ArgumentNullException(nameof(skillServiceRequest));

            _currentContext = new MessageContext
            {
                Text = GetText(skillServiceRequest),
                Intent = skillServiceRequest.Request.Intent.Name
            };

            foreach (var slot in skillServiceRequest.Request.Intent.Slots)
            {
                _currentContext.Slots[slot.Key] = slot.Value.Value;
            }

            if (skillServiceRequest.Request.Intent.Name == "ChangeState")
            {
                var mentionedArea = GetSlotValue(skillServiceRequest, "Area");
                var mentionedComponent = GetSlotValue(skillServiceRequest, "Component");
                var mentionedState = GetSlotValue(skillServiceRequest, "State");

                IdentifyAreas(mentionedArea);
                IdentifyComponents(mentionedComponent);
                IdentifyComponentStates(mentionedState);

                var areaFound = !string.IsNullOrEmpty(mentionedArea) && _currentContext.IdentifiedAreaIds.Any();
                if (!areaFound)
                {
                    // TODO: Extend logic and check which component identification leads to a precise value (Count == 1).
                }

                FilterComponentIds();
            }

            return _currentContext;
        }

        private string GetText(SkillServiceRequest skillServiceRequest)
        {
            var result = string.Empty;
            foreach (var slot in skillServiceRequest.Request.Intent.Slots)
            {
                result += slot.Value.Value + " ";
            }

            return result.Trim();
        }

        private string GetSlotValue(SkillServiceRequest skillServiceRequest, string slotName)
        {
            SkillServiceRequestRequestIntentSlot slot;
            if (!skillServiceRequest.Request.Intent.Slots.TryGetValue(slotName, out slot))
            {
                return null;
            }

            return slot.Value;
        }

        private void IdentifyAreas(string input)
        {
            foreach (var area in _areaService.GetAreas())
            {
                if (IsCaptionOrKeywordMatch(input, area.Settings.Caption, area.Settings.Keywords))
                {
                    _currentContext.IdentifiedAreaIds.Add(area.Id);
                }
            }
        }

        private void IdentifyComponents(string input)
        {
            foreach (var component in _componentService.GetComponents())
            {
                var componentSettings = _settingsService.GetSettings<ComponentSettings>(component.Id);

                if (IsCaptionOrKeywordMatch(input, componentSettings.Caption, componentSettings.Keywords))
                {
                    _currentContext.IdentifiedComponentIds.Add(component.Id);
                }
            }
        }

        private void IdentifyComponentStates(string input)
        {
            foreach (var stateSynonym in _stateSynonyms)
            {
                if (input.IndexOf(stateSynonym.Key, StringComparison.CurrentCultureIgnoreCase) > -1)
                {
                    // TODO: Ensure that the match is not part of a word. Check EOL etc.
                    _currentContext.IdentifiedComponentStates.Add(stateSynonym.Value);
                }
            }
        }

        private void FilterComponentIds()
        {
            if (_currentContext.IdentifiedComponentIds.Count == 1)
            {
                _currentContext.AffectedComponentIds.Add(_currentContext.IdentifiedComponentIds.First());
                return;
            }

            foreach (var componentId in _currentContext.IdentifiedComponentIds)
            {
                foreach (var areaId in _currentContext.IdentifiedAreaIds)
                {
                    var area = _areaService.GetArea(areaId);

                    if (area.ContainsComponent(componentId))
                    {
                        _currentContext.AffectedComponentIds.Add(componentId);
                        break;
                    }
                }
            }
        }

        private bool IsCaptionOrKeywordMatch(string input, string caption, string keywords)
        {
            if (string.IsNullOrEmpty(input))
            {
                return false;
            }

            if (!string.IsNullOrEmpty(caption))
            {
                if (input.IndexOf(caption, StringComparison.CurrentCultureIgnoreCase) > -1)
                {
                    return true;
                }
            }

            if (!string.IsNullOrEmpty(keywords))
            {
                var keywordsList = keywords.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                if (keywordsList.Any(k => input.IndexOf(k, StringComparison.CurrentCultureIgnoreCase) > -1))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
