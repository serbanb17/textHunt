using System;

namespace CoordinatorService.Models
{
    public class ResultResponseModel
    {
        public Guid SessionId { get; set; }
        public object Result { get; set; }
    }
}