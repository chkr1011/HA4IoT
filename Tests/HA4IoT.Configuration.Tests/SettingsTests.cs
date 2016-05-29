using System;
using Windows.Data.Json;
using FluentAssertions;
using HA4IoT.Core.Settings;
using HA4IoT.Networking;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace HA4IoT.Configuration.Tests
{
    [TestClass]
    public class SettingsTests
    {
        [TestMethod]
        public void SetAndRead_Settings()
        {
            var settings = new SettingsContainer(string.Empty);
            settings.SetValue("A", "A");
            settings.SetValue("B", 1);
            settings.SetValue("C", true);
            settings.SetValue("D", TimeSpan.Parse("12:30"));
            settings.SetValue("E", 1.1F);

            settings.GetString("A").ShouldBeEquivalentTo("A");
            settings.GetInteger("B").ShouldBeEquivalentTo(1);
            settings.GetBoolean("C").ShouldBeEquivalentTo(true);
            settings.GetTimeSpan("D").ShouldBeEquivalentTo(TimeSpan.Parse("12:30"));
            settings.GetFloat("E").ShouldBeEquivalentTo(1.1F);
        }

        [TestMethod]
        public void EventShouldBeFiredOnce_OnChange()
        {
            int firedCount = 0;

            var settings = new SettingsContainer(string.Empty);
            settings.ValueChanged += (s, e) => firedCount++;

            settings.SetValue("A", "A");
            firedCount.ShouldBeEquivalentTo(1);

            settings.SetValue("A", "B");

            firedCount.ShouldBeEquivalentTo(2);

            settings.SetValue("A", "B");
            firedCount.ShouldBeEquivalentTo(2);
        }

        [TestMethod]
        public void ImportSettingsShouldUpdateAndFireEvent()
        {
            int firedCount = 0;

            var settings = new SettingsContainer(string.Empty);
            settings.ValueChanged += (s, e) => firedCount++;

            settings.SetValue("A", "A");
            settings.SetValue("B", 1);
            settings.SetValue("C", true);

            firedCount.ShouldBeEquivalentTo(3);

            var importSource = new JsonObject();
            importSource.SetNamedString("A", "x");
            importSource.SetNamedNumber("B", 2);
            importSource.SetNamedBoolean("C", true);
            
            settings.Import(importSource);

            firedCount.ShouldBeEquivalentTo(5);

            settings.GetString("A").ShouldBeEquivalentTo("x");
            settings.GetInteger("B").ShouldBeEquivalentTo(2);
            settings.GetBoolean("C").ShouldBeEquivalentTo(true);
        }

        [TestMethod]
        public void ExportShouldMatchSettings()
        {
            var settings = new SettingsContainer(string.Empty);
            settings.SetValue("A", "A");
            settings.SetValue("B", 1);
            settings.SetValue("C", true);

            var exportComparison = new JsonObject();
            exportComparison.SetNamedString("A", "A");
            exportComparison.SetNamedNumber("B", 1);
            exportComparison.SetNamedBoolean("C", true);

            settings.Export().Stringify().ShouldBeEquivalentTo(exportComparison.Stringify());
        }
    }
}
