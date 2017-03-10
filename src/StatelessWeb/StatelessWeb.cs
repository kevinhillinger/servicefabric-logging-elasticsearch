using System.Collections.Generic;
using System.Fabric;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using StatelessWeb.Diagnostics;
using Serilog.Core.Enrichers;

namespace StatelessWeb
{
    /// <summary>
    /// The FabricRuntime creates an instance of this class for each service type instance. 
    /// </summary>
    internal sealed class StatelessWeb : StatelessService
    {
        private readonly EventFlowLoggerConfigurator loggerConfigurator;

        public StatelessWeb(StatelessServiceContext context, EventFlowLoggerConfigurator loggerConfigurator)
            : base(context)
        {
            this.loggerConfigurator = loggerConfigurator;
        }

        /// <summary>
        /// Optional override to create listeners (like tcp, http) for this service instance.
        /// </summary>
        /// <returns>The collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new ServiceInstanceListener[]
            {
                new ServiceInstanceListener(serviceContext =>
                    new WebListenerCommunicationListener(serviceContext, "ServiceEndpoint", url =>
                    {
                        ServiceEventSource.Current.ServiceMessage(serviceContext, $"Starting WebListener on {url}");
                        return GetWebHost(serviceContext, url);
                    }))
            };
        }

        IWebHost GetWebHost(StatelessServiceContext serviceContext, string url)
        {
            return new WebHostBuilder().UseWebListener()
                .ConfigureServices(services => {
                    services.AddSingleton<StatelessServiceContext>(serviceContext);

                    // stateless service properties included in each event being written from logger
                    loggerConfigurator.EnrichWith(new PropertyEnricher[]
                    {
                        new PropertyEnricher("ServiceTypeName", serviceContext.ServiceTypeName),
                        new PropertyEnricher("ServiceName", serviceContext.ServiceName),
                        new PropertyEnricher("PartitionId", serviceContext.PartitionId),
                        new PropertyEnricher("InstanceId", serviceContext.ReplicaOrInstanceId),
                    });

                    services.AddSingleton<EventFlowLoggerConfigurator>(loggerConfigurator);
                })
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>()
                .UseUrls(url)
                .Build();
        }
    }
}