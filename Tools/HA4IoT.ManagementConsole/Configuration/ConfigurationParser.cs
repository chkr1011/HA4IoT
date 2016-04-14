using System;
using System.Collections.Generic;
using HA4IoT.ManagementConsole.Configuration.ViewModels;
using Newtonsoft.Json.Linq;

namespace HA4IoT.ManagementConsole.Configuration
{
    public class ConfigurationParser
    {
        public IList<AreaItemVM> Parse(JObject configuration)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            var areas = new List<AreaItemVM>();
            foreach (JProperty area in ((JObject)configuration["areas"]).Properties())
            {
                areas.Add(new AreaParser(area).Parse());
            }

            areas.Sort((x, y) => x.SortValue.CompareTo(y.SortValue));

            return areas;
        }
    }
}
