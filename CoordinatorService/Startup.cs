using System;
using System.Collections;
using System.Collections.Concurrent;
using CoordinatorService.Commands;
using CoordinatorService.Interfaces;
using CoordinatorService.Managers;
using CoordinatorService.Models;
using CoordinatorService.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CoordinatorService
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var commandResults = new ConcurrentDictionary<Guid, object>();
            var config = ConfigurationService.GetConfiguration("Properties/configuration.json");
            var workersManager = new WorkersManager();
            var commandManager = new CommandManager();
            commandManager.RegisterCommand("Search", new SearchCommand(workersManager, config));
            commandManager.RegisterCommand("BuildIndex", new BuildIndexCommand(workersManager, config));

            services.Add(new ServiceDescriptor(typeof(IWorkersManager), workersManager));
            services.Add(new ServiceDescriptor(typeof(ICommandManager), commandManager));
            services.Add(new ServiceDescriptor(typeof(IDictionary), commandResults));
            services.Add(new ServiceDescriptor(typeof(ConfigurationModel), config));
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
