using Microsoft.Diagnostics.EventFlow;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Core;
using Serilog.Core.Enrichers;
using System;
using System.Collections.Generic;

namespace StatelessWeb.Diagnostics
{
    /// <summary>
    /// A configurator used to configure the logger (input) for the event flow diagnostic pipeline
    /// </summary>
    /// <remarks>
    /// The configurator creates a seem between the consumer of the logger (e.g. asp.net core) and the configuration of the logger
    /// </remarks>
    public class EventFlowLoggerConfigurator
    {
        private readonly DiagnosticPipeline diagnosticPipeline;
        private Serilog.ILogger logger;
        private List<PropertyEnricher> logEnrichments = new List<PropertyEnricher>();

        /// <summary>
        /// Gets the configured logger
        /// </summary>
        public Serilog.ILogger Logger
        {
            get
            {
                if (logger == null)
                {
                    throw new InvalidOperationException("Logger not configured");
                }
                return logger;
            }
        }

        public EventFlowLoggerConfigurator(DiagnosticPipeline diagnosticPipeline)
        {
            this.diagnosticPipeline = diagnosticPipeline;
        }

        public void EnrichWith(IEnumerable<PropertyEnricher> properties)
        {
            logEnrichments.AddRange(properties);
        }

        /// <summary>
        /// Configures logging
        /// </summary>
        /// <param name="loggerFactory"></param>
        public void Configure(IConfiguration configuration)
        {
            logger = new LoggerConfiguration()
                        .ReadFrom.Configuration(configuration)
                        .WriteTo.EventFlow(diagnosticPipeline)
                        .CreateLogger();

            if (logEnrichments.Count > 0)
            {
                logger = logger.ForContext(logEnrichments);
            }
        }
    }
}
