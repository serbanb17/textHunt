using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WorkerService.Commands;
using WorkerService.Interfaces;
using WorkerService.Managers;
using WorkerService.Models;
using WorkerService.Services;

namespace WorkerService
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
            var config = ConfigurationService.GetConfiguration("Properties/configuration.json");
            var commandManager = new CommandManager();
            commandManager.RegisterCommand("Stem", new StemCommand(config));
            commandManager.RegisterCommand("CountWords", new CountWordsCommand(config));
            commandManager.RegisterCommand("ComputeWeights", new ComputeWeightsCommand(config));
            commandManager.RegisterCommand("ComputeVector", new ComputeVectorCommand(config));
            commandManager.RegisterCommand("ComputeCosineSimilarity", new ComputeCosineSimilarityCommand(config));
            
            services.Add(new ServiceDescriptor(typeof(ICommandManager), commandManager));
            services.Add(new ServiceDescriptor(typeof(ConfigurationModel), config));
            services.AddControllers();

            CoordinatorCommManager.Register(config);
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
