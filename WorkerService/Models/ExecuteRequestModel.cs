using System.Collections.Generic;

namespace WorkerService.Models
{
    public class ExecuteRequestModel
    {
        public string Command { get; set; }
        public IEnumerable<object> Arguments { get; set; }
    }
}