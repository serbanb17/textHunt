using System;

namespace WorkerService.Models
{
    public class ResultResponseModel
    {
        public Guid SessionId { get; set; }
        public object Result { get; set; }
    }
}