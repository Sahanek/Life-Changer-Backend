using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace API.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        
        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (WebException ex)
            {
                await HandleExceptionAsync(httpContext, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, WebException exception)
        {
            context.Response.ContentType = "application/json";

            using (WebResponse response = exception.Response)
            {
                string ErrorMessage = "No message";
                HttpWebResponse httpWebResponse = (HttpWebResponse) response;

                using (Stream data = response.GetResponseStream())
                using (var reader = new StreamReader(data))
                {
                     ErrorMessage = reader.ReadToEnd();
                }

                return context.Response.WriteAsync(new Errors.ErrorDetails()
                {
                    StatusCode = (int)httpWebResponse.StatusCode,
                    Message = ErrorMessage
                }.ToString()) ;

            }

        }
        
    }
}
