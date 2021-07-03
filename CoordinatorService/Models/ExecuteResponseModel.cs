using System;

namespace CoordinatorService.Models
{
    public class ExecuteResponseModel
    {
        public bool Started { get; set; }
        public Guid SessionId { get; set; }
    }
}