using Microsoft.AspNetCore.Http;
using Serilog;
using Serilog.Events;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace StatelessWeb.Diagnostics
{
    class LoggingMiddleware
    {
        const string MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";

        private readonly HttpExceptionHandler exceptionHandler;
        readonly ILogger logger;
        readonly RequestDelegate next;

        public LoggingMiddleware(RequestDelegate next, HttpExceptionHandler exceptionHandler, ILogger logger)
        {
            if (next == null)
                throw new ArgumentNullException(nameof(next));

            this.next = next;
            this.exceptionHandler = exceptionHandler;
            this.logger = logger.ForContext<LoggingMiddleware>();
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (httpContext == null)
                throw new ArgumentNullException(nameof(httpContext));

            using (var loggerContext = new LoggerContext(httpContext))
            {
                loggerContext.Begin();

                try
                {
                    await next(httpContext);
                }
                catch (Exception e)
                {
                    await exceptionHandler.HandleAsync(httpContext, e);
                }

                await loggerContext.CaptureAsync();

                GetLogger(loggerContext).Write(loggerContext.Level, MessageTemplate,
                    loggerContext.Method,
                    loggerContext.Path,
                    loggerContext.StatusCode,
                    loggerContext.ElapsedMilliseconds);
            }
        }

        ILogger GetLogger(LoggerContext loggerContext)
        {
            var logger = this.logger;

            if (loggerContext.ResponseBody.Length > 0)
            {
                logger = logger.ForContext("ResponseBody", loggerContext.ResponseBody);
            }

            if (loggerContext.Level == LogEventLevel.Error)
            {
                var request = loggerContext.HttpContext.Request;

                var errorLogger = logger
                    .ForContext("RequestHeaders", request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()), destructureObjects: true)
                    .ForContext("RequestHost", request.Host)
                    .ForContext("RequestProtocol", request.Protocol);

                if (request.HasFormContentType)
                    errorLogger = errorLogger.ForContext("RequestForm", request.Form.ToDictionary(v => v.Key, v => v.Value.ToString()), destructureObjects: true);

                return errorLogger;
            }
            
            return logger;
        }
    }
}
