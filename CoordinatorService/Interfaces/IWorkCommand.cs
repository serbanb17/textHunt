using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace CoordinatorService.Interfaces
{
    public interface IWorkCommand
    {
        object Run(IEnumerable<object> args, ILogger logger);
    }
}