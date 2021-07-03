using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WorkerService.Interfaces;
using WorkerService.Models;
using WorkerService.Services;

namespace WorkerService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CommandController : ControllerBase
    {
        private readonly ICommandManager _commandManager;
        private readonly ConfigurationModel _config;
        private readonly ILogger<CommandController> _logger;

        private void ResultCallback(Guid id, object result)
        {
            var resultModel = new ResultResponseModel();
            resultModel.SessionId = id;
            resultModel.Result = result;
            _logger.LogInformation("{Class}::{Method} | SessionId = {SessionId}", nameof(CommandController), nameof(ResultCallback), id);
            UrlRequestService.PostRequest(_config.CoordinatorServiceUrl + "/workerendpoint/sendresult", resultModel, out string _);
        }

        public CommandController(ICommandManager commandManager, ConfigurationModel config, ILogger<CommandController> logger)
        {
            _commandManager = commandManager;
            _config = config;
            _logger = logger;
        }

        [HttpPost]
        [Route(nameof(Execute))]
        public IActionResult Execute([FromBody] ExecuteRequestModel req)
        {
            _logger.LogInformation("{Class}::{Method} | Command = {reqCommand}", nameof(CommandController), nameof(Execute), req.Command);
            var resp = new ExecuteResponseModel();
            var respTuple = _commandManager.Execute(req.Command, Guid.NewGuid(), result => ResultCallback(resp.SessionId, result), req.Arguments, _logger);
            resp.Started = respTuple.Item1;
            resp.SessionId = respTuple.Item2;

            return Ok(resp);
        }

        [HttpPost]
        [Route(nameof(IsBusy))]
        public IActionResult IsBusy()
        {
            // _logger.LogInformation("{Class}::{Method} | ", nameof(CommandController), nameof(IsBusy), _commandManager.IsBusy);
            return Ok(_commandManager.IsBusy);
        }
    }
}