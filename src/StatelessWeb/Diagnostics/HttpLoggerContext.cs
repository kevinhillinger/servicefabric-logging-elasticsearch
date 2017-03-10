using Microsoft.AspNetCore.Http;
using Serilog.Events;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace StatelessWeb.Diagnostics
{
    /// <summary>
    /// tracks the logging context for an HTTP request within the asp.net log middleware
    /// </summary>
    class LoggerContext : IDisposable
    {
        private HttpContext httpContext;
        private Stopwatch stopwatch;
        private Stream originalBody;
        private MemoryStream responseBuffer;
        private bool disposed = false;

        public HttpContext HttpContext { get { return httpContext; } }

        public LogEventLevel Level { get; private set; }
        public int? StatusCode { get; private set; }
        public string Method { get; private set; }
        public string Path { get; private set; }
        public double ElapsedMilliseconds { get; private set; }
        public string ResponseBody { get; private set; }

        public LoggerContext(HttpContext httpContext)
        {
            this.httpContext = httpContext;
        }

        public void Begin()
        {
            PrepareResponseBodyBuffer();
            StartNewStopwatch();
        }

        /// <summary>
        /// Captures the result of the http request
        /// </summary>
        /// <returns></returns>
        public async Task CaptureAsync()
        {
            StatusCode = httpContext.Response?.StatusCode;
            Level = StatusCode > 499 ? LogEventLevel.Error : LogEventLevel.Information;
            Method = httpContext.Request.Method;
            Path = httpContext.Request.Path;

            ElapsedMilliseconds = stopwatch.Elapsed.TotalMilliseconds;

            await CaptureResponseBodyAsync();
        }

        private void StartNewStopwatch()
        {
            this.stopwatch = Stopwatch.StartNew();
            stopwatch.Start();
        }

        private void PrepareResponseBodyBuffer()
        {
            this.originalBody = httpContext.Response.Body;
            this.responseBuffer = new MemoryStream();
            httpContext.Response.Body = responseBuffer;
        }

        private async Task CaptureResponseBodyAsync()
        {
            responseBuffer.Seek(0, SeekOrigin.Begin);
            await responseBuffer.CopyToAsync(originalBody);

            httpContext.Response.Body = originalBody;

            responseBuffer.Seek(0, SeekOrigin.Begin);
            using (var reader = new StreamReader(responseBuffer))
            {
                ResponseBody = await reader.ReadToEndAsync();
            }

            responseBuffer.Dispose();
        }

        public void Dispose()
        {
            if (disposed) return;

            originalBody.Dispose();
            responseBuffer.Dispose();
            disposed = true;
        }
    }
}
