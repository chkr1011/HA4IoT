using System;

namespace HA4IoT.Contracts.Components
{
    public class ComponentFeatureNotSupportedException : Exception
    {
        public ComponentFeatureNotSupportedException(Type typeOfFeature)
            : base($"Feature '{typeOfFeature}' is not supported.")
        {
            TypeOfFeature = typeOfFeature;
        }

        public Type TypeOfFeature { get; }
    }
}
