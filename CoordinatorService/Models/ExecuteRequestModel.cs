using System.Collections.Generic;

namespace CoordinatorService.Models
{
    public class ExecuteRequestModel
    {
        public string Command { get; set; }
        public IEnumerable<object> Arguments { get; set; }
    }
}