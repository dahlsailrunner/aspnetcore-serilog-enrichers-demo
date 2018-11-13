using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Threading.Tasks;
using Serilog;

namespace SimpleApi
{
    public class CustomErrorMiddleware
    {
        private readonly RequestDelegate next;

        public CustomErrorMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context /* other dependencies */)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var code = HttpStatusCode.InternalServerError; // 500 if unexpected
            
            Log.Error(exception, exception.Message);

            var result = JsonConvert.SerializeObject(new
            {
                error = "An error occurred in our API.  Please refer to the error id below with our support team.",
                id = context.TraceIdentifier
            });
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)code;
            return context.Response.WriteAsync(result);
        }
    }
}
