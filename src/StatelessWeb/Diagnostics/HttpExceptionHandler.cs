using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Threading.Tasks;

namespace StatelessWeb.Diagnostics
{
    /// <summary>
    /// Exception handler for handling uncaught exceptions coming from controllers
    /// </summary>
    class HttpExceptionHandler
    {
        public async Task HandleAsync(HttpContext context, Exception exception)
        {
            var code = HttpStatusCode.InternalServerError; // 500 if unexpected

            // TODO: should place custom exception handling and messaging here for handling 403 Unauthorized, etc.
            // TODO: determine the level/content of error messages for an API response in this location as well

            var result = JsonConvert.SerializeObject(new { error = exception.Message });

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)code;

            await context.Response.WriteAsync(result);
        }
    }
}
