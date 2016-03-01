using System;
using Windows.Data.Json;
using HA4IoT.Contracts.Networking;
using HA4IoT.Contracts.WeatherStation;
using HA4IoT.Networking;

namespace HA4IoT.Hardware.OpenWeatherMapWeatherStation
{
    public class OWMWeatherStationHttpApiDispatcher
    {
        private readonly OWMWeatherStation _weatherStation;
        private readonly IHttpRequestController _httpApiController;

        public OWMWeatherStationHttpApiDispatcher(OWMWeatherStation weatherStation, IHttpRequestController httpApiController)
        {
            if (weatherStation == null) throw new ArgumentNullException(nameof(weatherStation));
            if (httpApiController == null) throw new ArgumentNullException(nameof(httpApiController));

            _weatherStation = weatherStation;
            _httpApiController = httpApiController;
        }

        public void ExposeToApi()
        {
            _httpApiController.HandleGet("weatherStation").Using(HandleApiGet);
            _httpApiController.HandlePost("weatherStation").Using(HandleApiPost);
        }

        private void HandleApiPost(HttpContext context)
        {
            JsonObject requestData;
            if (JsonObject.TryParse(context.Request.Body, out requestData))
            {
                context.Response.StatusCode = HttpStatusCode.BadRequest;
                return;
            }
            
            var situationValue = requestData.GetNamedString("situation", WeatherSituation.Unknown.ToString());
            var situation = (WeatherSituation)Enum.Parse(typeof (WeatherSituation), situationValue, true);

            var temperature = (float)requestData.GetNamedNumber("temperature");
            var humidity = (float)requestData.GetNamedNumber("humidity");
            var sunrise = TimeSpan.Parse(requestData.GetNamedString("sunrise"));
            var sunset = TimeSpan.Parse(requestData.GetNamedString("sunset"));

            _weatherStation.SetStatus(situation, temperature, humidity, sunrise, sunset);
        }

        private void HandleApiGet(HttpContext httpContext)
        {
            httpContext.Response.Body = new JsonBody(_weatherStation.ExportStatusToJsonObject());
        }
    }
}
