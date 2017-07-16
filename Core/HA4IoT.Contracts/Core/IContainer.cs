using System;
using System.Collections.Generic;

namespace HA4IoT.Contracts.Core
{
    public interface IContainer
    {
        TContract GetInstance<TContract>() where TContract : class;

        IList<TContract> GetInstances<TContract>() where TContract : class;

        void RegisterSingleton<TImplementation>() where TImplementation : class;

        void RegisterSingleton<TImplementation>(Func<TImplementation> instanceCreator) where TImplementation : class;

        void RegisterSingleton<TContract, TImplementation>() where TContract : class
            where TImplementation : class, TContract;
    }
}