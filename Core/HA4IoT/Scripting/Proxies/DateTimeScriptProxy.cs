using System;
using HA4IoT.Contracts.Scripting;
using HA4IoT.Contracts.Services.System;
using MoonSharp.Interpreter;

namespace HA4IoT.Scripting.Proxies
{
    public class DateTimeScriptProxy : IScriptProxy
    {
        private readonly IDateTimeService _dateTimeService;

        [MoonSharpHidden]
        public DateTimeScriptProxy(IDateTimeService dateTimeService)
        {
            _dateTimeService = dateTimeService ?? throw new ArgumentNullException(nameof(dateTimeService));
        }

        [MoonSharpHidden]
        public string Name => "dateTime";

        public string Now()
        {
            return _dateTimeService.Now.ToString("O");
        }

        public double Hour()
        {
            return _dateTimeService.Now.Hour;
        }

        public double Minute()
        {
            return _dateTimeService.Now.Minute;
        }

        public double Second()
        {
            return _dateTimeService.Now.Second;
        }

        public string DayOfWeek()
        {
            return _dateTimeService.Now.DayOfWeek.ToString();
        }

        public double Day()
        {
            return _dateTimeService.Now.Day;
        }

        public double Month()
        {
            return _dateTimeService.Now.Month;
        }

        public double Year()
        {
            return _dateTimeService.Now.Year;
        }
    }
}
