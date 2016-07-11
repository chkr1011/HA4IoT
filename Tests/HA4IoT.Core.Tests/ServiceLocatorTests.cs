using System;
using Windows.Data.Json;
using FluentAssertions;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Tests.Mockups;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace HA4IoT.Core.Tests
{
    [TestClass]
    public class ServiceLocatorTests
    {
        [TestMethod]
        public void ServiceLocator_RegisterServices()
        {
            var serviceLocator = new ServiceLocator();
            serviceLocator.RegisterService(typeof(IDaylightService), new TestDaylightService());
            serviceLocator.RegisterService(typeof(IDateTimeService), new TestDateTimeService());

            serviceLocator.GetServices().Count.ShouldBeEquivalentTo(2);
        }

        [TestMethod]
        public void ServiceLocator_RegisterServices_Duplicate_Direct()
        {
            var serviceLocator = new ServiceLocator();
            serviceLocator.RegisterService(typeof(IDaylightService), new TestDaylightService());
            serviceLocator.RegisterService(typeof(IDateTimeService), new TestDateTimeService());

            try
            {
                serviceLocator.RegisterService(typeof(IDateTimeService), new DateTimeService());

                throw new Exception("Exception not thrown.");
            }
            catch (Exception)
            {
                // OK!
            }
        }

        [TestMethod]
        public void ServiceLocator_RegisterServices_Duplicate_WithInterface()
        {
            var serviceLocator = new ServiceLocator();
            serviceLocator.RegisterService(typeof(IDaylightService), new TestDaylightService());
            serviceLocator.RegisterService(typeof(IDateTimeService), new TestDateTimeService());

            try
            {
                serviceLocator.RegisterService(typeof(IDateTimeService), new DuplicateDateTimeService());

                throw new Exception("Exception not thrown.");
            }
            catch (Exception)
            {
                // OK!
            }
        }

        [TestMethod]
        public void ServiceLocator_GetServices_Direct()
        {
            var serviceLocator = new ServiceLocator();
            serviceLocator.RegisterService(typeof(TestDaylightService), new TestDaylightService());
            serviceLocator.RegisterService(typeof(TestDateTimeService), new TestDateTimeService());

            TestDateTimeService dts;
            serviceLocator.TryGetService(out dts).ShouldBeEquivalentTo(true);
        }

        [TestMethod]
        public void ServiceLocator_TryGetServices_WithInterface()
        {
            var serviceLocator = new ServiceLocator();
            serviceLocator.RegisterService(typeof(IDaylightService), new TestDaylightService());
            serviceLocator.RegisterService(typeof(IDateTimeService), new TestDateTimeService());

            IDateTimeService dts;
            serviceLocator.TryGetService(out dts).ShouldBeEquivalentTo(true);
        }

        [TestMethod]
        public void ServiceLocator_GetServices_WithInterface()
        {
            var serviceLocator = new ServiceLocator();
            serviceLocator.RegisterService(typeof(IDaylightService), new TestDaylightService());
            serviceLocator.RegisterService(typeof(IDateTimeService), new TestDateTimeService());

            serviceLocator.GetService<IDateTimeService>();
        }

        private class DuplicateDateTimeService : IDateTimeService
        {
            public JsonObject ExportStatusToJsonObject()
            {
                throw new NotImplementedException();
            }

            public void HandleApiCommand(IApiContext apiContext)
            {
                throw new NotImplementedException();
            }

            public void HandleApiRequest(IApiContext apiContext)
            {
                throw new NotImplementedException();
            }

            public void CompleteRegistration(IServiceLocator serviceLocator)
            {
                throw new NotImplementedException();
            }

            public DateTime GetDate()
            {
                throw new NotImplementedException();
            }

            public TimeSpan GetTime()
            {
                throw new NotImplementedException();
            }

            public DateTime GetDateTime()
            {
                throw new NotImplementedException();
            }
        }
    }
}
