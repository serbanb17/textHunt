using System.Collections.Generic;
using CoordinatorService.Interfaces;
using CoordinatorService.Models;
using Microsoft.Extensions.Logging;

namespace CoordinatorService.Commands
{
    public partial class BuildIndexCommand : IWorkCommand
    {
        IWorkersManager _workersManager;
        private readonly ConfigurationModel _config;
        public BuildIndexCommand(IWorkersManager workersManager, ConfigurationModel config)
        {
            _workersManager = workersManager;
            _config = config;
        }
        public object Run(IEnumerable<object> args, ILogger logger)
        {
            logger.LogInformation("{Class}::{Method} | Call {Method2}()", nameof(BuildIndexCommand), nameof(Run), nameof(InitDb));
            if(!InitDb(logger)) 
                return "fail";
            
            logger.LogInformation("{Class}::{Method} | Call {Method2}()", nameof(BuildIndexCommand), nameof(Run), nameof(Stem));
            Stem(logger);
            
            logger.LogInformation("{Class}::{Method} | Call {Method2}()", nameof(BuildIndexCommand), nameof(Run), nameof(CountWords));
            CountWords(logger);
            
            logger.LogInformation("{Class}::{Method} | Call {Method2}()", nameof(BuildIndexCommand), nameof(Run), nameof(ComputeWeights));
            ComputeWeights(logger);
            
            logger.LogInformation("{Class}::{Method} | Call {Method2}()", nameof(BuildIndexCommand), nameof(Run), nameof(ComputeVectors));
            ComputeVectors(logger);
            
            logger.LogInformation("{Class}::{Method} | return success", nameof(BuildIndexCommand), nameof(Run));
            return "success";
        }
    }
}