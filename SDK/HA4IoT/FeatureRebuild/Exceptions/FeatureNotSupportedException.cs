using System;

namespace HA4IoT.FeatureRebuild.Exceptions
{
    public class FeatureNotSupportedException : Exception
    {
        public FeatureNotSupportedException(Type featureType)
            : base($"The requested feature of type '{featureType}' is not supported.")
        {
        }
    }
}
