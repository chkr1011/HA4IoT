using System;
using HA4IoT.Contracts.Components;
using HA4IoT.Features;

namespace HA4IoT.Components
{
    public class Component
    {
        public Component(ComponentId id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));

            Id = id;
        }

        public ComponentId Id { get; }
        
        public FeatureCollection Features { get; } = new FeatureCollection();

        public ComponentSettings Settings { get; } = new ComponentSettings();
    }
}
