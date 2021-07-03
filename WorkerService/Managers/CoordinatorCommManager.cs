using System.Threading;
using WorkerService.Models;
using WorkerService.Services;

namespace WorkerService.Managers
{
    public class CoordinatorCommManager
    {
        public static void Register(ConfigurationModel config)
        {
            while(!UrlRequestService.PostRequest(
                config.CoordinatorServiceUrl + "/workerendpoint/register", 
                config.MyUrl, 
                out string result))
            {
                Thread.Sleep(5000);
            }
        }
    }
}