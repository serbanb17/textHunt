using System;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Threading;
using CoordinatorService.Interfaces;
using CoordinatorService.Models;
using CoordinatorService.Services;

namespace CoordinatorService.Managers
{
    public class WorkersManager : IWorkersManager
    {
        private ConcurrentDictionary<string, string> _workers;
        private ConcurrentDictionary<Guid, Action<object>> _inWork;
        public WorkersManager()
        {
            _workers = new ConcurrentDictionary<string, string>();
            _inWork = new ConcurrentDictionary<Guid, Action<object>>();
        }
        public bool DoWork(string urlAppend, object reqBody, Action<object> callback)
        {
            bool ok = false;
            string freeWorker = null;
            while (freeWorker is null)
            {
                // LogInformation($"WorkersManager::DoWork; Before foreach; freeWorker={freeWorker}");
                foreach (var workerUrl in _workers.Keys)
                {
                    // LogInformation($"WorkersManager::DoWork; Before UrlRequest; workerUrl={workerUrl}");
                    bool urlReq = UrlRequestService.PostRequest(workerUrl + "/command/isbusy", "", out string result);
                    // LogInformation($"WorkersManager::DoWork after UrlRequest; workerUrl={workerUrl} urlReq={urlReq}  result={result}");
                    if (urlReq
                    && result == "false")
                    {
                        freeWorker = workerUrl;
                        if(UrlRequestService.PostRequest(workerUrl + urlAppend, reqBody, out string result2))
                        {
                            var response = JsonSerializer.Deserialize<ExecuteResponseModel>(result2, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                            if(response.Started)
                            {
                                _inWork[response.SessionId] = callback;
                                ok = true;
                            }
                        }
                        break;
                    }
                }
            }
            return ok;
        }

        public void ProcessResult(ResultResponseModel result)
        {
            if (!_inWork.ContainsKey(result.SessionId)) return;
            _inWork[result.SessionId]?.Invoke(result.Result);
            _inWork.TryRemove(result.SessionId, out Action<object> _);
        }

        public void WaitAllResults()
        {
            while(_inWork.Count > 0)
                Thread.Sleep(1000);
        }

        public bool Register(string workerUrl)
        {
            if (!_workers.ContainsKey(workerUrl))
            {
                _workers[workerUrl] = null;
                return true;
            }

            return false;
        }

        public Action<string> LogInformation {get;set;}
    }
}