using System;
using System.Collections;
using CoordinatorService.Interfaces;
using CoordinatorService.Models;
using CoordinatorService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CoordinatorService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CommandController : ControllerBase
    {
        private readonly ICommandManager _commandManager;
        IWorkersManager _workersManager;
        IDictionary _commandResults;
        private readonly ILogger<CommandController> _logger;
        private ConfigurationModel _config;

        public CommandController(ICommandManager commandManager, IWorkersManager workersManager, IDictionary commandResults, ILogger<CommandController> logger, ConfigurationModel config)
        {
            _commandManager = commandManager;
            _workersManager = workersManager;
            _commandResults = commandResults;
            _logger = logger;
            _config = config;
        }

        private void SaveResult(Guid id, object result)
        {
            _commandResults[id] = result;
        }

        [HttpPost]
        [Route(nameof(Execute))]
        public IActionResult Execute([FromBody] ExecuteRequestModel req)
        {
            _logger.LogInformation("{Class}::{Method} | Command = {reqCommand}", nameof(CommandController), nameof(Execute), req.Command);
            var resp = new ExecuteResponseModel();
            var respTuple = _commandManager.Execute(req.Command, Guid.NewGuid(), result => SaveResult(resp.SessionId, result), req.Arguments, _logger);
            resp.Started = respTuple.Item1;
            resp.SessionId = respTuple.Item2;
            return Ok(resp);
        }

        [HttpPost]
        [Route(nameof(GetResult))]
        public IActionResult GetResult([FromBody] Guid id)
        {
            _logger.LogInformation("{Class}::{Method} | id = {id}", nameof(CommandController), nameof(GetResult), id);
            object result = null;
            if(_commandResults.Contains(id))
            {
                result = _commandResults[id];
                _commandResults.Remove(id);
            }
            return Ok(result);
        }

        [HttpPost]
        [Route(nameof(IsBusy))]
        public IActionResult IsBusy()
        {
            _logger.LogInformation("{Class}::{Method} | ", nameof(CommandController), nameof(IsBusy), _commandManager.IsBusy);
            return Ok(_commandManager.IsBusy);
        }

        [HttpPost]
        [Route(nameof(GetItemUrl))]
        public IActionResult GetItemUrl([FromBody] string id)
        {
            
            return(Ok(MongoUtilService.GetItemUrl(_config, id)));
        }
    }
}