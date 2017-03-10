using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using StatelessWeb.Diagnostics;

namespace StatelessWeb
{
    public class Startup
    {
        private readonly Serilog.ILogger logger;

        public IConfigurationRoot Configuration { get; }

        public Startup(IHostingEnvironment env, EventFlowLoggerConfigurator loggerConfigurator)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
            loggerConfigurator.Configure(Configuration);

            logger = loggerConfigurator.Logger;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            // if using a different DI container, then hook up accordingly. for simplicity purposes, we're using the built-in IServiceCollection
            services.AddSingleton(logger);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            //environment automatically set from ASPNETCORE_ENVIRONMENT variable
            //env.EnvironmentName = EnvironmentName.Development;

            ConfigureLogging(app, env, loggerFactory);

            app.UseMiddleware<LoggingMiddleware>(logger);
            app.UseMvc();
        }

        void ConfigureLogging(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/error");
            }

            loggerFactory.AddDebug();
            loggerFactory.AddSerilog(logger, dispose: true);
        }
    }
}
