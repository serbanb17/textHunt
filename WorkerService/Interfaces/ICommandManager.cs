using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace WorkerService.Interfaces
{
    public interface ICommandManager
    {
        bool IsBusy { get; }
        Guid CalledId { get; }
        bool RegisterCommand(string commandName, IWorkCommand command);
        Tuple<bool, Guid> Execute(string commandName, Guid callerId, Action<object> callback, IEnumerable<object> args, ILogger logger);
    }
}