using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Web.Http.Filters;
using HA4IoT.CloudApi.Services;

namespace HA4IoT.CloudApi.Controllers
{
    public class ExceptionFilter : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext context)
        {
            if (context.Exception is UnauthorizedAccessException)
            {
                context.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
                return;
            }

            if (context.Exception is ControllerNotReachableException)
            {
                context.Response = new HttpResponseMessage(HttpStatusCode.BadGateway);
                return;
            }

            Trace.WriteLine("EXCEPTION:" + context.Exception);
            context.Response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
        }
    }
}