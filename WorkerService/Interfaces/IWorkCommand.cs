using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace WorkerService.Interfaces
{
    public interface IWorkCommand
    {
        object Run(IEnumerable<object> args, ILogger logger);
    }
}