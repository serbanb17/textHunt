using System;
using CoordinatorService.Models;

namespace CoordinatorService.Interfaces
{
    public interface IWorkersManager
    {
        bool Register(string workerUrl);
        bool DoWork(string urlAppend, object reqBody, Action<object> callback);
        void ProcessResult(ResultResponseModel result);
        void WaitAllResults();
    }
}