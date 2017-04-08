using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Filters;
using HA4IoT.CloudApi.Services.Exceptions;

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

            var httpResponseException = context.Exception as HttpResponseException;
            if (httpResponseException != null)
            {
                Trace.WriteLine($"EXCEPTION ({httpResponseException.Response.StatusCode}): " + context.Exception);
                context.Response = httpResponseException.Response;
            }
            else
            {
                Trace.WriteLine("EXCEPTION:" + context.Exception);
                context.Response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
        }
    }
}