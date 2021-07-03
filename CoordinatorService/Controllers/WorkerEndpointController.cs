using CoordinatorService.Interfaces;
using CoordinatorService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CoordinatorService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WorkerEndpointController : ControllerBase
    {
        IWorkersManager _workersManager;
        private readonly ILogger<WorkerEndpointController> _logger;

        public WorkerEndpointController(IWorkersManager workersManager, ILogger<WorkerEndpointController> logger)
        {
            _workersManager = workersManager;
            _logger = logger;
        }

        [HttpPost]
        [Route(nameof(Register))]
        public IActionResult Register([FromBody] string address)
        {
            _logger.LogInformation("{Class}::{Method} | Address = {Address}", nameof(WorkerEndpointController), nameof(Register), address);
            _workersManager.Register(address);
            return Ok();
        }

        [HttpPost]
        [Route(nameof(SendResult))]
        public IActionResult SendResult([FromBody] ResultResponseModel result)
        {
            _logger.LogInformation("{Class}::{Method} | SessionId = {SessionId}", nameof(WorkerEndpointController), nameof(SendResult), result.SessionId);
            _workersManager.ProcessResult(result);
            return Ok();
        }
    }
}