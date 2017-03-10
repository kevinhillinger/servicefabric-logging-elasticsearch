using Microsoft.Diagnostics.EventFlow;
using Microsoft.Diagnostics.EventFlow.ServiceFabric;
using Microsoft.ServiceFabric.Services.Runtime;
using Serilog;
using Serilog.Events;
using StatelessWeb.Diagnostics;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace StatelessWeb
{
    internal static class Program
    {
        /// <summary>
        /// This is the entry point of the service host process.
        /// </summary>
        private static void Main()
        {
            try
            {
                // **** Instantiate log collection via EventFlow
                // 
                using (var diagnosticsPipeline = ServiceFabricDiagnosticPipelineFactory.CreatePipeline("StatelessWebType-DiagnosticsPipeline"))
                {
                    var loggerConfigurator = new EventFlowLoggerConfigurator(diagnosticsPipeline);

                    ServiceRuntime.RegisterServiceAsync("StatelessWebType", context => new StatelessWeb(context, loggerConfigurator)).GetAwaiter().GetResult();
                    ServiceEventSource.Current.ServiceTypeRegistered(Process.GetCurrentProcess().Id, typeof(StatelessWeb).Name);

                    KeepServiceRunning();
                }
            }
            catch (Exception e)
            {
                ServiceEventSource.Current.ServiceHostInitializationFailed(e.ToString());
                throw;
            }
        }

        static void KeepServiceRunning() { Thread.Sleep(Timeout.Infinite);  }
    }
}
