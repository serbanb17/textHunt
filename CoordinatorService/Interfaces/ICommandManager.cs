using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace CoordinatorService.Interfaces
{
    public interface ICommandManager
    {
        bool IsBusy { get; }
        Guid CallerId { get; }
        bool RegisterCommand(string commandName, IWorkCommand command);
        Tuple<bool, Guid> Execute(string commandName, Guid callerId, Action<object> callback, IEnumerable<object> args, ILogger logger);
    }
}